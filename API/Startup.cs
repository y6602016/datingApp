using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.Extensions;
using API.Interfaces;
using API.Middleware;
using API.Services;
using API.SignalR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

// In Startup.cs, you can also set up all of your configurations which tells 
// the framework how you want the application to start and execute.
namespace API
{
  public class Startup
  {
    private readonly IConfiguration _config;
    public Startup(IConfiguration config)
    {
      _config = config;
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      // use extension method here, this method is defined in extensions folder
      services.AddAppLicationServices(_config);

      services.AddControllers();
      services.AddCors();

      // use extension method here, this method is defined in extensions folder
      services.AddIdentityService(_config);

      // register signalR here
      services.AddSignalR();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      app.UseMiddleware<ExceptionMiddleware>();

      app.UseHttpsRedirection();

      app.UseRouting();

      // add UseCors just after UseRouting() and before UseAuthentication()
      // specify the source from Angular url
      app.UseCors(policy => policy.AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        .WithOrigins("https://localhost:4200"));

      // add UseAuthentication just before UseAuthorization
      app.UseAuthentication();

      app.UseAuthorization();

      // use index html, which is the built client angular file
      app.UseDefaultFiles();
      app.UseStaticFiles();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();

        // add signalR to routing as well
        endpoints.MapHub<PresenceHub>("hubs/presence");

        // add message to routing
        endpoints.MapHub<MessageHub>("hubs/message");

        // server can't recongize client url, it only knows the api url
        // the client url defined in angular routing should be fallbacked to the FallbackController
        endpoints.MapFallbackToController("Index", "Fallback");
      });
    }
  }
}
