using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
  [ApiController]
  [Route("api/[controller]")]

  // the base class for inheritance use
  public class BaseApiController : ControllerBase { }
}