using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Services;
using API.SignalR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions
{
  // this extension class is used for IServiceCollection in Startup.cs file
  // we set dependency injection regestration and dbcontext registration in the method

  // we won't use this class to create instances, so we declare it static class
  // who calss this function is the first parameter "this ... "
  public static class ApplicationServiceExtensions
  {
    public static IServiceCollection AddAppLicationServices(this IServiceCollection services, IConfiguration config)
    {
      // add signalR presenceTracker, it has a shared dict to record all online username and their connectionId
      services.AddSingleton<PresenceTracker>();

      // config cloudinary settings by implement CloudinarySettings class and use "CloudinarySettings" in appssetting.json file
      services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));

      // !!!dependency injection, regestration here!!!
      // token service should use AddScoped, which has lifetime of http request
      // we use the token in APIController, and once request come in, this service injected into the
      // particular controller, then the service instance is created, and when the request finishes, service ends
      services.AddScoped<ITokenService, TokenService>();

      // register photo service
      services.AddScoped<IPhotoService, PhotoService>();

      // create LogUserActivity service (also need to add it into the BaseApiController)
      services.AddScoped<LogUserActivity>();

      // ====== we dont need to inject repositories since we can use unit of work ======
      // register like service
      // services.AddScoped<ILikesRepository, LikesRepository>();

      // register IMessageRepository injection
      // services.AddScoped<IMessageRepository, MessageRepository>();

      // register IUserRepository injection
      // services.AddScoped<IUserRepository, UserRepository>();

      // inject unit of work to replace above three repositories
      services.AddScoped<IUnitOfWork, UnitOfWork>();
      // ==============================================

      services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);

      // register and add dbcontext here for our program use, then we can use the ORM
      services.AddDbContext<DataContext>(options =>
      {
        // options.UseNpgsql(config.GetConnectionString("DefaultConnection")); // connection string is set in appsettings.Dev file
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        string connStr;

        // Depending on if in development or production, use either Heroku-provided
        // connection string, or development connection string from env var.
        if (env == "Development")
        {
          // Use connection string from file.
          connStr = config.GetConnectionString("DefaultConnection");
        }
        else
        {
          // Use connection string provided at runtime by Heroku.
          var connUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

          // Parse connection URL to connection string for Npgsql
          connUrl = connUrl.Replace("postgres://", string.Empty);
          var pgUserPass = connUrl.Split("@")[0];
          var pgHostPortDb = connUrl.Split("@")[1];
          var pgHostPort = pgHostPortDb.Split("/")[0];
          var pgDb = pgHostPortDb.Split("/")[1];
          var pgUser = pgUserPass.Split(":")[0];
          var pgPass = pgUserPass.Split(":")[1];
          var pgHost = pgHostPort.Split(":")[0];
          var pgPort = pgHostPort.Split(":")[1];

          connStr = $"Server={pgHost};Port={pgPort};User Id={pgUser};Password={pgPass};Database={pgDb};SSL Mode=Require;TrustServerCertificate=True";
        }

        // Whether the connection string came from the local development configuration file
        // or from the environment variable from Heroku, use it to set up your DbContext.
        options.UseNpgsql(connStr);
      });

      return services;
    }
  }
}