using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
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
    private readonly IPhotoService _photoService;

    // inject repository and mapper in constuctor
    public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
    {
      _photoService = photoService;
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
    // the Name attribute for add-photo api use
    [HttpGet("{username}", Name = "GetUser")]
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
      // we use customized claim principal method define in ClaimsPrincipalExtensions to get username
      var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

      // map memberUpdateDto to the user object
      _mapper.Map(memberUpdateDto, user);

      // then update _userRepository and let it tracks the user
      _userRepository.Update(user);

      // now we save the update user
      if (await _userRepository.SaveAllAsync()) return NoContent();

      return BadRequest("Failed to update the user");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
      var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());
      var result = await _photoService.AddPhotoAsync(file);
      // if there is an error, return it as BadRequest
      if (result.Error != null)
      {
        return BadRequest(result.Error.Message);
      }

      // use the received url and publicId to create a new Photo object
      var photo = new Photo
      {
        Url = result.SecureUrl.AbsoluteUri,
        PublicId = result.PublicId
      };

      // if user has no photo before, then this new photo is the main photo
      if (user.Photos.Count == 0)
      {
        photo.IsMain = true;
      }

      user.Photos.Add(photo);

      // save if then return photoDto, convert photo to photoDto by Map
      if (await _userRepository.SaveAllAsync())
      {
        // "GetUser" is the Name defind in HttpGet("{username}" endpoint
        // we need to return CreatedAtRoute since it returns 201 status code instead of 200
        // moreover, it's header contains the Location of the url with the uploaded image
        // we need return this url to client so that users can find their images
        return CreatedAtRoute("GetUser", new { Username = user.UserName }, _mapper.Map<PhotoDto>(photo));
      }

      return BadRequest("Problem adding photo");
    }

    [HttpPut("set-main-photo/{photoId}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
      var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());
      var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

      if (photo.IsMain)
      {
        return BadRequest("This is already your main photo");
      }

      var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
      if (currentMain != null)
      {
        // set the original main false
        currentMain.IsMain = false;
      }
      // set new photo IsMain true
      photo.IsMain = true;

      if (await _userRepository.SaveAllAsync())
      {
        return NoContent();
      }

      return BadRequest("Failed to set main photo");
    }

    [HttpDelete("delete-photo/{photoId}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
      var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

      var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
      if (photo == null)
      {
        return NotFound();
      }
      if (photo.IsMain)
      {
        return BadRequest("You cannot delete your main photo");
      }

      if (photo.PublicId != null)
      {
        var result = await _photoService.DeletePhotoAsync(photo.PublicId);
        if (result.Error != null)
        {
          return BadRequest(result.Error.Message);
        }
      }
      // untrack the photo from the photo context
      user.Photos.Remove(photo);

      // really delete ans save
      if (await _userRepository.SaveAllAsync())
      {
        return Ok();
      }

      return BadRequest("Fail to delete the photo");
    }
  }
}