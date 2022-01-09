using API.Data;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
  // make the controller authrized 
  [Authorize]
  public class UsersController : BaseApiController
  {
    private readonly IUserRepository _userRepository;
    public UsersController(IUserRepository userRepository)
    {
      _userRepository = userRepository;
    }

    // get all users
    // api/users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
    {
      // we can return <List<AppUser>> as well, same thing.
      // but the List containing too many methods, we don't neet them
      // we just need a simple iterable user list, so use IEnumerable 

      // letting the method being async can increase performance
      // to do so, we need to add async and return task<>

      // use DataContext to interact with db and query the users object (we define Users method in DataContext)
      // then convert the users to list, since method is async, we need to use await
      // and since we use await, we need to use ToListAsync

      // update: use repository to interact with DbContext in controller
      return Ok(await _userRepository.GetUsersAsync());
    }

    // get the specific user
    // api/users/id
    // [HttpGet("{id}")]
    // public async Task<ActionResult<AppUser>> GetUser(int id)
    // {
    //   // only one specific user, instead returning enumarable objects, return one entity

    //   // use the id parameter to find the user
    //   return Ok(await _userRepository.GetUserAsync(id));
    // }

    // get the specific user
    // api/users/username
    [HttpGet("{username}")]
    public async Task<ActionResult<AppUser>> GetUser(string username)
    {
      // only one specific user, instead returning enumarable objects, return one entity

      // use the username parameter to find the user
      return Ok(await _userRepository.GetUserByUsernameAsync(username));
    }
  }
}