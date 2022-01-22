
using API.DTOs;
using API.Entities;
using API.Extensions;
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

    public async Task<IEnumerable<LikeDto>> GetUserLikes(string predicate, int userId)
    {
      // based on the prdicate, get the corresponding results

      var users = _context.Users.OrderBy(u => u.UserName).AsQueryable();
      var likes = _context.Likes.AsQueryable();

      if (predicate == "liked")
      {
        // userId here is the sourceUser, find all people he likes
        // take userId as sourceId to search the like relationship
        likes = likes.Where(like => like.SourceUserId == userId);
        // then select the likedUser as users
        users = likes.Select(like => like.LikedUser);
      }

      if (predicate == "likedBy")
      {
        // userId here is the likedUser, find all people who likes him
        // take userId as sourceId to search the like relationship
        likes = likes.Where(like => like.LikedUserId == userId);
        // then select the likedUser as users
        users = likes.Select(like => like.SourceUser);
      }
      return await users.Select(user => new LikeDto
      {
        Username = user.UserName,
        KnownAs = user.KnownAs,
        Age = user.DateOfBirth.CaculateAge(),
        PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain).Url,
        Id = user.Id
      }).ToListAsync();
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