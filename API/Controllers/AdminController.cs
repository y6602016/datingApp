
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
  public class AdminController : BaseApiController
  {
    private readonly UserManager<AppUser> _userManager;
    public AdminController(UserManager<AppUser> userManager)
    {
      _userManager = userManager;
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("users-with-roles")]
    public async Task<ActionResult> GetUserWithRoles()
    {
      var users = await _userManager.Users
          .Include(r => r.UserRoles)
          .ThenInclude(r => r.Role)
          .OrderBy(u => u.UserName)
          .Select(u => new // anonymous abject projected to return
          {
            u.Id,
            Username = u.UserName,
            Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
          })
          .ToListAsync();

      return Ok(users);
    }

    [HttpPost("edit-roles/{username}")]
    public async Task<ActionResult> EditRole(string username, [FromQuery] string roles)
    {
      // user name from root parameter, roles from query string\

      // split the query string with "," and thern convert to array
      var selectedRoles = roles.Split(",").ToArray();

      var user = await _userManager.FindByNameAsync(username);

      // check user exists or not
      if (user == null)
      {
        return NotFound("Could not find the user");
      }

      var userRoles = await _userManager.GetRolesAsync(user);

      // add roles for the user, except it's original role
      var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

      if (!result.Succeeded)
      {
        return BadRequest("Failed to add to roles");
      }

      // then remove the original roles
      result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

      if (!result.Succeeded)
      {
        return BadRequest("Failed to remove from roles");
      }

      return Ok(await _userManager.GetRolesAsync(user));
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("photos-to-moderate")]
    public ActionResult GetPhotosForModeration()
    {
      return Ok("Admins or moderators can see this");
    }
  }
}