using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class UsersController : ControllerBase
  {
    private readonly DataContext _context;
    public UsersController(DataContext context)
    {
      _context = context;
    }

    // get all users
    // api/users
    [HttpGet]
    public ActionResult<IEnumerable<AppUser>> GetUsers()
    {
      // we can return <List<AppUser>> as well, same thing.
      // but the List containing too many methods, we don't neet them
      // we just need a simple iterable user list, so use IEnumerable 

      // use DataContext to interact with db and query the users object (we define Users method in DataContext)
      // then convert the users to list
      return _context.Users.ToList();
    }

    // get the specific user
    // api/users/id
    [HttpGet("{id}")]
    public ActionResult<AppUser> GetUser(int id)
    {
      // only ione specific user, instead returning enumarable objects, return one entity

      // use the id parameter to find the user
      return _context.Users.Find(id);
    }
  }
}