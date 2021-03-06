
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
  public class MessageHub : Hub
  {
    // private readonly IMessageRepository _messageRepository;
    private readonly IMapper _mapper;
    // private readonly IUserRepository _userRepository;
    private readonly IHubContext<PresenceHub> _presenceHub;
    private readonly PresenceTracker _tracker;
    private readonly IUnitOfWork _unitOfWork;

    public MessageHub(IMapper mapper, IUnitOfWork unitOfWork, IHubContext<PresenceHub> presenceHub, PresenceTracker tracker)
    {
      _unitOfWork = unitOfWork;
      // _messageRepository = messageRepository;
      _mapper = mapper;
      // _userRepository = userRepository;
      _presenceHub = presenceHub;
      _tracker = tracker;
    }


    public override async Task OnConnectedAsync()
    {
      var httpContext = Context.GetHttpContext();

      // the "user" key in the query will correspond the other user's username as value
      var otherUser = httpContext.Request.Query["user"].ToString();

      // put current user and other user to compare and get groupName
      var groupName = GetGroupName(Context.User.GetUsername(), otherUser);

      // Groups is a property as a group manager in Hub
      await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

      var group = await AddToGroup(groupName);

      // update group
      await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

      var messages = await _unitOfWork.MessageRepository.GetMessageThread(Context.User.GetUsername(), otherUser);

      // after we've retrieved message including unread message, they are marked as read in above GetMessageThread call
      // so the repository has changes, then we call .HasChanges() to check there are changes or not. if true, save it
      if (_unitOfWork.HasChanges())
      {
        await _unitOfWork.Complete();
      }

      // call ReceiveMessageThread to send the message the group member
      await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
      var group = await RemoveFromMessageGroup();
      await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
      await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
      var username = Context.User.GetUsername();

      if (username == createMessageDto.RecipientUsername.ToLower())
      {
        throw new HubException("You cannot send messages to yourself");
      }

      var sender = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
      var recipient = await _unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

      if (recipient == null)
      {
        throw new HubException("Not Found User");
      }

      var message = new Message
      {
        Sender = sender,
        Recipient = recipient,
        SenderUsername = username,
        RecipientUsername = recipient.UserName,
        Content = createMessageDto.Content
      };
      var groupName = GetGroupName(sender.UserName, recipient.UserName);

      var group = await _unitOfWork.MessageRepository.GetMessageGroup(groupName);

      // if the recipient is at the same group, then mark the message as "read"
      if (group.Connections.Any(x => x.Username == recipient.UserName))
      {
        message.DateRead = DateTime.UtcNow;
      }
      else
      {
        var connections = await _tracker.GetConnectionsForUser(recipient.UserName);
        // if the recipient is online but not connecting to the same group
        if (connections != null)
        {
          await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived",
            new { username = sender.UserName, knownAs = sender.KnownAs });
        }
      }

      _unitOfWork.MessageRepository.AddMessage(message);
      if (await _unitOfWork.Complete())
      {
        await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
      }
    }

    private async Task<Group> AddToGroup(string groupName)
    {
      var group = await _unitOfWork.MessageRepository.GetMessageGroup(groupName);
      var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());

      if (group == null)
      {
        group = new Group(groupName);
        _unitOfWork.MessageRepository.AddGroup(group);
      }

      group.Connections.Add(connection);

      // if the recipient join the group, mark the message as "read"
      if (await _unitOfWork.Complete())
      {
        return group;
      }

      throw new HubException("Failed to join group");
    }

    private async Task<Group> RemoveFromMessageGroup()
    {
      var group = await _unitOfWork.MessageRepository.GetGroupForConnection(Context.ConnectionId);
      var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
      _unitOfWork.MessageRepository.RemoveConnection(connection);
      if (await _unitOfWork.Complete())
      {
        return group;
      }
      throw new HubException("Failed to remove from group");
    }

    private string GetGroupName(string caller, string other)
    {
      // < 0 means caller less than other
      var stringCompare = string.CompareOrdinal(caller, other) < 0;
      return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }
  }
}