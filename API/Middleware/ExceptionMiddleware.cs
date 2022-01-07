using System.Net;
using System.Text.Json;
using API.Errors;

// we deasign custom exception middleware to handle error and exceptions
// we use it in startup.cs file
namespace API.Middleware
{
  public class ExceptionMiddleware
  {
    // RequestDelegate next is what's coming next in middleware pipe
    // ILogger can log exception to terminal
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;
    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger,
        IHostEnvironment env)
    {
      _env = env;
      _logger = logger;
      _next = next;
    }

    // invoke async in middleware
    public async Task InvokeAsync(HttpContext context)
    {
      try
      {
        // put the context into next to process
        await _next(context);
      }
      catch (Exception ex)
      {
        // log the error
        _logger.LogError(ex, ex.Message);
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        // check in dev mode or not
        var response = _env.IsDevelopment()
            ? new ApiException(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString())
            : new ApiException(context.Response.StatusCode, "Internal Server Error");

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        var json = JsonSerializer.Serialize(response, options);

        await context.Response.WriteAsJsonAsync(json);
      }
    }
  }
}