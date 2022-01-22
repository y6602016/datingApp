
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
  public class LikesRepository : ILikesRepository
  {
    private readonly DataContext _context;
    public LikesRepository(DataContext context)
    {
      _context = context;
    }

    public async Task<UserLike> GetUserLike(int sourceUserId, int likedUserId)
    {
      return await _context.Likes.FindAsync(sourceUserId, likedUserId);
    }

    public async Task<PageList<LikeDto>> GetUserLikes(LikesParams likeparams)
    {
      // based on the prdicate, get the corresponding results

      var users = _context.Users.OrderBy(u => u.UserName).AsQueryable();
      var likes = _context.Likes.AsQueryable();

      if (likeparams.Predicate == "liked")
      {
        // userId here is the sourceUser, find all people he likes
        // take userId as sourceId to search the like relationship
        likes = likes.Where(like => like.SourceUserId == likeparams.UserId);
        // then select the likedUser as users
        users = likes.Select(like => like.LikedUser);
      }

      if (likeparams.Predicate == "likedBy")
      {
        // userId here is the likedUser, find all people who likes him
        // take userId as sourceId to search the like relationship
        likes = likes.Where(like => like.LikedUserId == likeparams.UserId);
        // then select the likedUser as users
        users = likes.Select(like => like.SourceUser);
      }
      var likedUsers = users.Select(user => new LikeDto
      {
        Username = user.UserName,
        KnownAs = user.KnownAs,
        Age = user.DateOfBirth.CaculateAge(),
        PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain).Url,
        Id = user.Id
      });

      return await PageList<LikeDto>.CreateAsync(likedUsers, likeparams.PageNumber, likeparams.PageSize);
    }

    // get the current user and all users the current user likes
    public async Task<AppUser> GetUserWithLikes(int userId)
    {
      return await _context.Users
        .Include(x => x.LikedUsers)
        .FirstOrDefaultAsync(x => x.Id == userId);
    }
  }
}