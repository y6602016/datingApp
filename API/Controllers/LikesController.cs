using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
  [Authorize]
  public class LikesController : BaseApiController
  {
    // private readonly IUserRepository _userRespository;
    // private readonly ILikesRepository _likeRepository;
    private readonly IUnitOfWork _unitOfWork;
    public LikesController(IUnitOfWork unitOfWork)
    {
      _unitOfWork = unitOfWork;
      // _likeRepository = likeRepository;
      // _userRespository = userRespository;
    }

    [HttpPost("{username}")]
    public async Task<ActionResult> AddLike(string username)
    {
      var sourceUserId = User.GetUserId();
      var likedUser = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
      var sourceUser = await _unitOfWork.LikesRepository.GetUserWithLikes(sourceUserId);

      // check the likedUser exists or not
      if (likedUser == null)
      {
        return NotFound();
      }

      // check the user likes himself or not
      if (sourceUser.UserName == username)
      {
        return BadRequest("You cannot like yourself!");
      }

      var userLike = await _unitOfWork.LikesRepository.GetUserLike(sourceUserId, likedUser.Id);

      // if userLike exists, means already like
      if (userLike != null)
      {
        return BadRequest("You already like this user");
      }

      userLike = new UserLike
      {
        SourceUserId = sourceUserId,
        LikedUserId = likedUser.Id
      };

      sourceUser.LikedUsers.Add(userLike);

      if (await _unitOfWork.Complete())
      {
        return Ok();
      }

      return BadRequest("Failed to like user");
    }


    [HttpGet]
    public async Task<ActionResult<IEnumerable<LikeDto>>> GtUserLikes([FromQuery] LikesParams likesParams)
    {
      likesParams.UserId = User.GetUserId();
      var users = await _unitOfWork.LikesRepository.GetUserLikes(likesParams);

      Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
      return Ok(users);
    }
  }
}