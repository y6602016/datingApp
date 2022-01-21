using API.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
  [ServiceFilter(typeof(LogUserActivity))] // apply the service here such that it update user's lastActive on all controllers 
  [ApiController]
  [Route("api/[controller]")]

  // the base class for inheritance use
  public class BaseApiController : ControllerBase { }
}