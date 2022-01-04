using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Interfaces;
using API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

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
      // !!!dependency injection, regestration here!!!
      // token service should use AddScoped, which has lifetime of http request
      // we use the token in APIController, and once request come in, this service injected into the
      // particular controller, then the service instance is created, and when the request finishes, service ends
      services.AddScoped<ITokenService, TokenService>();

      // register and add dbcontext here for our program use, then we can use the ORM
      services.AddDbContext<DataContext>(options =>
      {
        options.UseSqlite(_config.GetConnectionString("DefaultConnection")); // connection string is set in appsettings.Dev file
      });

      services.AddControllers();
      services.AddCors();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }

      app.UseHttpsRedirection();

      app.UseRouting();

      // add UseCors just after routing and before authorization
      // specify the source from Angular url
      app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:4200"));

      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
      });
    }
  }
}
