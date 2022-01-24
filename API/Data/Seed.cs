
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
  public static class Seed
  {
    // we'll use this class to seed data in program.cs file
    public static async Task SeedUser(DataContext context)
    {
      // if db has any user, just return
      if (await context.Users.AnyAsync()) return;
      // if there is no user, then we read the file to import user
      var userData = await System.IO.File.ReadAllTextAsync("Data/UserSeedData.json");
      var users = JsonSerializer.Deserialize<List<AppUser>>(userData);
      foreach (var user in users)
      {
        // ------- removed since we've used IdentityUser ----------
        // using var hmac = new HMACSHA512();
        // user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Pa$$w0rd"));
        // user.PasswordSalt = hmac.Key;
        // add user to context track, not really add to db
        // ------- removed since we've used IdentityUser ----------

        user.UserName = user.UserName.ToLower();
        context.Users.Add(user);
      }

      await context.SaveChangesAsync();
    }

  }
}