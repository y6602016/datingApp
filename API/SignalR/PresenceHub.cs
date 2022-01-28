using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
  public class PresenceHub : Hub
  {
    [Authorize]
    public override async Task OnConnectedAsync()
    {
      // send message when online
      await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUsername());
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
      // send message when offline
      await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername());

      // if there is an exception, pass to parent class
      await base.OnDisconnectedAsync(exception);
    }
  }
}