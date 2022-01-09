using API.DTOs;
using API.Entities;

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
    Task<IEnumerable<MemberDto>> GetMembersAsync();

    Task<MemberDto> GetMemberAsync(string username);
  }
}