using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace API
{
  public class Program
  {
    public static async Task Main(string[] args)
    {
      // to seed the data, we build the host first, then create scope

      // first call CreateHostBuilder to create service, call ConfigureServices() and injection 
      // register work now, then call .Build() we'll get a "host"(aka. app), now the host call
      // Configure() to add middleware ,routing rules and endpoints...etc. Then it can Run()
      var host = CreateHostBuilder(args).Build();
      using var scope = host.Services.CreateScope();
      var services = scope.ServiceProvider; // service provider = injection container
      try
      {
        // extract the context instance from the injection container
        // we need the context instance to migrate the db
        var context = services.GetRequiredService<DataContext>();
        // apply migration the db
        await context.Database.MigrateAsync();

        // extract the UserManager and RoleManager instances from the injection container
        // we need them to seed the data
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
        // call Seed class and use it's SeedUser method to seed the data
        await Seed.SeedUser(userManager, roleManager);
      }
      catch (Exception ex)
      {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during migration");
      }
      await host.RunAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
              webBuilder.UseStartup<Startup>();
            });
  }
}
