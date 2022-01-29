
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
    private readonly IMessageRepository _messageRepository;
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;

    public MessageHub(IMessageRepository messageRepository, IMapper mapper, IUserRepository userRepository)
    {
      _messageRepository = messageRepository;
      _mapper = mapper;
      _userRepository = userRepository;
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

      await AddToGroup(groupName);

      var messages = await _messageRepository.GetMessageThread(Context.User.GetUsername(), otherUser);

      // call ReceiveMessageThread to send the message the group member
      await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
      await RemoveFromMessageGroup();
      await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
      var username = Context.User.GetUsername();

      if (username == createMessageDto.RecipientUsername.ToLower())
      {
        throw new HubException("You cannot send messages to yourself");
      }

      var sender = await _userRepository.GetUserByUsernameAsync(username);
      var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

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

      var group = await _messageRepository.GetMessageGroup(groupName);

      // mark the message as "read"
      if (group.Connections.Any(x => x.Username == recipient.UserName))
      {
        message.DateRead = DateTime.UtcNow;
      }

      _messageRepository.AddMessage(message);
      if (await _messageRepository.SavaAllAsync())
      {
        await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
      }
    }

    private async Task<bool> AddToGroup(string groupName)
    {
      var group = await _messageRepository.GetMessageGroup(groupName);
      var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());

      if (group == null)
      {
        group = new Group(groupName);
        _messageRepository.AddGroup(group);
      }

      group.Connections.Add(connection);

      return await _messageRepository.SavaAllAsync();
    }

    private async Task RemoveFromMessageGroup()
    {
      var connection = await _messageRepository.GetConnection(Context.ConnectionId);
      _messageRepository.RemoveConnection(connection);
      await _messageRepository.SavaAllAsync();
    }

    private string GetGroupName(string caller, string other)
    {
      // < 0 means caller less than other
      var stringCompare = string.CompareOrdinal(caller, other) < 0;
      return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }
  }
}