
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

    public MessageHub(IMessageRepository messageRepository, IMapper mapper)
    {
      _messageRepository = messageRepository;
      _mapper = mapper;
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

      var messages = await _messageRepository.GetMessageThread(Context.User.GetUsername(), otherUser);

      // call ReceiveMessageThread to send the message the group member
      await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
      await base.OnDisconnectedAsync(exception);
    }

    private string GetGroupName(string caller, string other)
    {
      // < 0 means caller less than other
      var stringCompare = string.CompareOrdinal(caller, other) < 0;
      return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }
  }
}