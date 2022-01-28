using API.Extensions;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
  public class PresenceHub : Hub
  {
    public override async Task OnConnectedAsync()
    {
      await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUsername());
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
      await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername());

      // if there is an exception, pass to parent class
      await base.OnDisconnectedAsync(exception);
    }
  }
}