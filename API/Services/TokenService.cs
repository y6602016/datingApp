using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Entities;
using API.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace API.Services
{
  // implement tokenService interface
  public class TokenService : ITokenService
  {
    private readonly SymmetricSecurityKey _key;
    // contructor use config's TokenKey converted to byte array and pass it 
    // to create an instance key of SymmetricSecurityKey
    public TokenService(IConfiguration config)
    {
      _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
    }

    // now we use the created key and username to create JWT token
    public string CreateToken(AppUser user)
    {
      // create token = tokenHandler + tokenDescripter
      // tokenDescripter = claims + credentials + expiretime

      // create claims
      var claims = new List<Claim>{
          new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
          new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
      };

      // create credentials
      var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

      // now we can use the claims and credentials to define the token
      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.Now.AddDays(7),
        SigningCredentials = creds
      };

      // create a tokenHandler
      var tokenHandler = new JwtSecurityTokenHandler();

      // now we can create the token!!
      var token = tokenHandler.CreateToken(tokenDescriptor);

      return tokenHandler.WriteToken(token);
    }
  }
}