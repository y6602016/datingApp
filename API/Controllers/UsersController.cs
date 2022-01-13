using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
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
    private readonly IMapper _mapper;

    // inject repository and mapper in constuctor
    public UsersController(IUserRepository userRepository, IMapper mapper)
    {
      _mapper = mapper;
      _userRepository = userRepository;
    }

    // get all users
    // api/users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
    {
      // we can return <List<AppUser>> as well, same thing.
      // but the List containing too many methods, we don't neet them
      // we just need a simple iterable user list, so use IEnumerable 

      // letting the method being async can increase performance
      // to do so, we need to add async and return task<>

      // use DataContext to interact with db and query the users object (we define Users method in DataContext)
      // then convert the users to list, since method is async, we need to use await
      // and since we use await, we need to use ToListAsync

      // update: get repository then put it into mapper to get returnable dto

      // var users = await _userRepository.GetUsersAsync();
      // var usersToReturn = _mapper.Map<IEnumerable<MemberDto>>(users);
      // return Ok(usersToReturn);

      // <optimized>update: map objects to member DTO in repository, just call GetMembersAsync method
      var users = await _userRepository.GetMembersAsync();
      return Ok(users);
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
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
      // only one specific user, instead returning enumarable objects, return one entity
      // use the username parameter to find the user
      // update: get repository then put it into mapper to get returnable dto
      // var user = await _userRepository.GetMemberAsync(username);

      // return _mapper.Map<MemberDto>(user);

      // <optimized>update: objects are mapped to member DTO in repository via automapper QueryableExtensions
      // just call GetMemberAsync method
      return await _userRepository.GetMemberAsync(username);
    }


    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
      // take username from the token that the API uses to authenticate
      var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

      var user = await _userRepository.GetUserByUsernameAsync(username);

      // map memberUpdateDto to the user object
      _mapper.Map(memberUpdateDto, user);

      // then update _userRepository and let it tracks the user
      _userRepository.Update(user);

      // now we save the update user
      if (await _userRepository.SaveAllAsync()) return NoContent();

      return BadRequest("Failed to update the user");
    }
  }
}