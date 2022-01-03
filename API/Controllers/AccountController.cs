
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
  public class AccountController : BaseApiController
  {
    private readonly DataContext _context;
    public AccountController(DataContext context)
    {
      _context = context;
    }


    [HttpPost("register")]
    public async Task<ActionResult<AppUser>> Register(RegisterDTO registerDTO)
    {
      // if the request is sent by body, we'll receive JSON object, but our parameters
      // are string, it will fail. if we use query string, then it's ok.
      // in order to receive username and password successfully, we need to use DTO

      // first check the username is unique or not
      if (await UserExists(registerDTO.Username)) return BadRequest("Username is taken");

      // HMACSHA512() return dispose, so we use "using"
      // hmac is used for hashing the password
      using var hmac = new HMACSHA512();

      var user = new AppUser
      {
        // make it to lower for UserExists method to check unique
        UserName = registerDTO.Username.ToLower(),
        // ComputeHash() takes byte[] as parameter, so convert password into byte
        PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password)),
        // HMACSHA512() initialize an instance with a key, so just get the key from hmac
        PasswordSalt = hmac.Key
      };

      // here not actually save entity into db, we just "tracking" it in entity framework
      _context.Users.Add(user);

      // here is exactly we save the entity into db
      await _context.SaveChangesAsync();

      return user;
    }

    // check user name unique or not
    private async Task<bool> UserExists(string username)
    {
      // AnyAsync() can check any match the passed object or not
      return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
    }
  }
}