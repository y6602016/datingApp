using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
  public class PresenceHub : Hub
  {
    private readonly PresenceTracker _tracker;
    public PresenceHub(PresenceTracker tracker)
    {
      _tracker = tracker;
    }

    [Authorize]
    public override async Task OnConnectedAsync()
    {
      // record the username and connectionId into the tracker(a shared dict)
      await _tracker.UserConnected(Context.User.GetUsername(), Context.ConnectionId);

      // call UserIsOnline to other users
      await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUsername());

      // get all online users
      var currentUsers = await _tracker.GetOnlineUsers();

      // call GetOnlineUsers to all online users
      await Clients.All.SendAsync("GetOnlineUsers", currentUsers);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
      await _tracker.UserDisconnected(Context.User.GetUsername(), Context.ConnectionId);

      // call UserIsOffline to other users
      await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername());

      // get all online users
      var currentUsers = await _tracker.GetOnlineUsers();

      // call GetOnlineUsers to all online users
      await Clients.All.SendAsync("GetOnlineUsers", currentUsers);

      // if there is an exception, pass to parent class
      await base.OnDisconnectedAsync(exception);
    }
  }
}