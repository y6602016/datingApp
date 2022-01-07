// DTO is the abstraction layer between client and DB.
// We can define DTO to define the object backend receive, and we also can hide 
// specific properties of data schema from the client. we can define DTO with only
// some properties instead of all data schema to client

// DTO is actually the property we receive from client, so adding validation in DTO
// layer is reasonable
using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
  public class RegisterDto
  {
    [Required]
    public string Username { get; set; }

    [Required]
    [StringLength(8, MinimumLength = 4)]
    // if we want to limit the password format, we can add [RegularExpression] here
    public string Password { get; set; }
  }
}