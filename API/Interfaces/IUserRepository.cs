using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
  public interface IUserRepository
  {
    void Update(AppUser user);

    Task<bool> SaveAllAsync();

    Task<IEnumerable<AppUser>> GetUsersAsync();

    Task<AppUser> GetUserAsync(int id);

    Task<AppUser> GetUserByUsernameAsync(string username);

    // we may not need to use repository in controller then map to Dto
    // we may directly map to Dto in repository here
    Task<PageList<MemberDto>> GetMembersAsync(UserParams userParams);

    Task<MemberDto> GetMemberAsync(string username);
  }
}