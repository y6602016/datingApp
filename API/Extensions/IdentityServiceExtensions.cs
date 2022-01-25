using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
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
      // configure all required setting of IdentityUser in here
      services.AddIdentityCore<AppUser>(opt =>
      {
        opt.Password.RequireNonAlphanumeric = false;
      })
        .AddRoles<AppRole>()
        .AddRoleManager<RoleManager<AppRole>>()
        .AddSignInManager<SignInManager<AppUser>>()
        .AddRoleValidator<RoleValidator<AppRole>>()
        .AddEntityFrameworkStores<DataContext>();

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

      // add role permission policy (authorization)
      services.AddAuthorization(opt =>
      {
        // theses two policy applied on the AdminController author policy
        opt.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
        opt.AddPolicy("MOderatePhotoRole", policy => policy.RequireRole("Admin", "Moderator"));
      });

      return services;
    }
  }
}