
using System.Security.Claims;

namespace API.Extensions
{
  public static class ClaimsPrincipalExtensions
  {
    // customize a User claim method 
    public static string GetUsername(this ClaimsPrincipal user)
    {
      return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
  }
}