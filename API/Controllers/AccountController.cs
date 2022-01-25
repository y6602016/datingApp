
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
  public class AccountController : BaseApiController
  {
    // private readonly DataContext _context;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;

    // dependency injection: inject token service 
    public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService, IMapper mapper)
    {
      _signInManager = signInManager;
      _userManager = userManager;
      _mapper = mapper;
      _tokenService = tokenService;
      // _context = context;
    }


    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDTO)
    {
      // if the request is sent by body, we'll receive a JSON object, but our parameters
      // are string, it will fail. if we use query string, then it's ok.
      // in order to receive username and password successfully, we need to use DTO

      // first check the username is unique or not
      if (await UserExists(registerDTO.Username)) return BadRequest("Username is taken");

      // use the mapper to create a user object
      var user = _mapper.Map<AppUser>(registerDTO);

      // make it to lower for UserExists method to check unique
      user.UserName = registerDTO.Username.ToLower();

      // create user with usermanager
      var result = await _userManager.CreateAsync(user, registerDTO.Password);

      if (!result.Succeeded)
      {
        return BadRequest(result.Errors);
      }

      // add role for the new user
      var roleResult = await _userManager.AddToRoleAsync(user, "Member");

      if (!roleResult.Succeeded)
      {
        return BadRequest(result.Errors);
      }

      // ------- removed since we've used IdentityUser ----------
      // // HMACSHA512() return dispose, so we use "using"
      // // hmac is used for hashing the password
      // using var hmac = new HMACSHA512();

      // // ComputeHash() takes byte[] as parameter, so convert password into byte
      // user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password));
      // // HMACSHA512() initialize an instance with a key, so just get the key from hmac
      // user.PasswordSalt = hmac.Key;

      // // here not actually save entity into db, we just "tracking" it in entity framework
      // _context.Users.Add(user);

      // // here is exactly we save the entity into db
      // await _context.SaveChangesAsync();
      // ------- removed since we've used IdentityUser ----------

      // return the client a new userDto
      return new UserDto
      {
        Username = user.UserName,
        Token = await _tokenService.CreateToken(user),
        KnownAs = user.KnownAs,
        Gender = user.Gender
      };
    }


    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
      // SingleOrDefaultAsync throws an exception if more than one element satisfies the condition
      var user = await _userManager.Users
        .Include(p => p.Photos)
        .SingleOrDefaultAsync(x => x.UserName == loginDto.Username.ToLower());
      if (user == null) return Unauthorized("Invalid username");

      // user login with signInManager, use Dto's password as password, false = we don't lock out user if they fail login
      var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

      if (!result.Succeeded)
      {
        return Unauthorized();
      }

      // ------- removed since we've used IdentityUser ----------
      // // now we check the password, we first use the key to find hmac
      // // take user.PasswordSalt as the key for hash
      // using var hmac = new HMACSHA512(user.PasswordSalt);

      // // then we use the input password to find the computedHash
      // var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

      // // then we compare two hashed password
      // // since it's byte[], we iterate the hashed password and compare every bit
      // for (int i = 0; i < computedHash.Length; i++)
      // {
      //   if (computedHash[i] != user.PasswordHash[i]) { return Unauthorized("Invalid Password"); }
      // }
      // ------- removed since we've used IdentityUser ----------

      // return the client a new userDto
      return new UserDto
      {
        Username = user.UserName,
        Token = await _tokenService.CreateToken(user),
        PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
        KnownAs = user.KnownAs,
        Gender = user.Gender
      };
    }

    // check user name unique or not
    private async Task<bool> UserExists(string username)
    {
      // AnyAsync() can check any match the passed object or not
      return await _userManager.Users.AnyAsync(x => x.UserName == username.ToLower());
    }
  }
}