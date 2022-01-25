
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
  public static class Seed
  {
    // we'll use this class to seed data in program.cs file
    public static async Task SeedUser(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
      // if db has any user, just return
      if (await userManager.Users.AnyAsync()) return;
      // if there is no user, then we read the file to import user
      var userData = await System.IO.File.ReadAllTextAsync("Data/UserSeedData.json");
      var users = JsonSerializer.Deserialize<List<AppUser>>(userData);

      if (users == null)
      {
        return;
      }

      // create role list with three roles
      var roles = new List<AppRole>{
        new AppRole{Name = "Member"},
        new AppRole{Name = "Admin"},
        new AppRole{Name = "Moderator"}
      };

      // create roles by iterating the role list
      foreach (var role in roles)
      {
        await roleManager.CreateAsync(role);
      }

      foreach (var user in users)
      {
        // ------- removed since we've used IdentityUser ----------
        // using var hmac = new HMACSHA512();
        // user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Pa$$w0rd"));
        // user.PasswordSalt = hmac.Key;
        // add user to context track, not really add to db
        // ------- removed since we've used IdentityUser ----------

        user.UserName = user.UserName.ToLower();
        await userManager.CreateAsync(user, "Pa$$w0rd");
        await userManager.AddToRoleAsync(user, "Member");
      }
      // mannualy create an admin user
      var admin = new AppUser
      {
        UserName = "admin"
      };

      await userManager.CreateAsync(admin, "Pa$$w0rd");
      await userManager.AddToRolesAsync(admin, new[] { "Admin", "Moderator" });
    }

  }
}