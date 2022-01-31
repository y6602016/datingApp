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
        options.UseSqlite(config.GetConnectionString("DefaultConnection")); // connection string is set in appsettings.Dev file
      });

      return services;
    }
  }
}