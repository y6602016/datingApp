
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
  public class AccountController : BaseApiController
  {
    private readonly DataContext _context;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    // dependency injection: inject token service 
    public AccountController(DataContext context, ITokenService tokenService, IMapper mapper)
    {
      _mapper = mapper;
      _tokenService = tokenService;
      _context = context;
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

      // HMACSHA512() return dispose, so we use "using"
      // hmac is used for hashing the password
      using var hmac = new HMACSHA512();

      // make it to lower for UserExists method to check unique
      user.UserName = registerDTO.Username.ToLower();
      // ComputeHash() takes byte[] as parameter, so convert password into byte
      user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password));
      // HMACSHA512() initialize an instance with a key, so just get the key from hmac
      user.PasswordSalt = hmac.Key;

      // here not actually save entity into db, we just "tracking" it in entity framework
      _context.Users.Add(user);

      // here is exactly we save the entity into db
      await _context.SaveChangesAsync();

      // return the client a new userDto
      return new UserDto
      {
        Username = user.UserName,
        Token = _tokenService.CreateToken(user),
        KnownAs = user.KnownAs,
        Gender = user.Gender
      };
    }


    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
      // SingleOrDefaultAsync throws an exception if more than one element satisfies the condition
      var user = await _context.Users
        .Include(p => p.Photos)
        .SingleOrDefaultAsync(x => x.UserName == loginDto.Username.ToLower());
      if (user == null) return Unauthorized("Invalid username");

      // now we check the password, we first use the key to find hmac
      // take user.PasswordSalt as the key for hash
      using var hmac = new HMACSHA512(user.PasswordSalt);

      // then we use the input password to find the computedHash
      var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

      // then we compare two hashed password
      // since it's byte[], we iterate the hashed password and compare every bit
      for (int i = 0; i < computedHash.Length; i++)
      {
        if (computedHash[i] != user.PasswordHash[i]) { return Unauthorized("Invalid Password"); }
      }

      // return the client a new userDto
      return new UserDto
      {
        Username = user.UserName,
        Token = _tokenService.CreateToken(user),
        PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
        KnownAs = user.KnownAs,
        Gender = user.Gender
      };
    }

    // check user name unique or not
    private async Task<bool> UserExists(string username)
    {
      // AnyAsync() can check any match the passed object or not
      return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
    }
  }
}