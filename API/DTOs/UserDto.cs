// this DTO is the user DTO return to client

namespace API.DTOs
{
  public class UserDto
  {
    public string Username { get; set; }
    public string Token { get; set; }
    public string PhotoUrl { get; set; }
  }
}