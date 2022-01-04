using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace API.Extensions
{
  // this extension class is used for IServiceCollection in Startup.cs file
  // we set AddAuthentication in the method

  // we won't use this class to create instances, so we declare it static class
  // who calss this function is the first parameter "this ... "
  public static class IdentityServiceExtensions
  {
    public static IServiceCollection AddIdentityService(this IServiceCollection services, IConfiguration config)
    {
      // add auth middleware to authenticate the token
      services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
          options.TokenValidationParameters = new TokenValidationParameters
          {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"])),
            ValidateIssuer = false, // Issuer = API server
            ValidateAudience = false, // Audience = Angular
          };
        });

      return services;
    }
  }
}