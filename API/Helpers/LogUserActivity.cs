// implement action filter
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Helpers
{
  public class LogUserActivity : IAsyncActionFilter
  {
    //two params means: execute the action (context) and then do something after this is executed (next)
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
      // hold the context that we get from the "next"
      var resultContext = await next();

      // if not auth, return
      if (!resultContext.HttpContext.User.Identity.IsAuthenticated) return;

      // use userId to get user, if we use userName to get user, we need to process include photos
      // which is not neccessary, so we use userId to call GetUserByIdAsync to get user
      var userId = resultContext.HttpContext.User.GetUserId();
      var repo = resultContext.HttpContext.RequestServices.GetService<IUnitOfWork>();
      var user = await repo.UserRepository.GetUserByIdAsync(userId);

      // update lastActive
      user.LastActive = DateTime.UtcNow;

      // save status
      await repo.Complete();
    }
  }
}