namespace API.DTOs
{
  public class MemberDto
  {
    public int Id { get; set; }

    // Username "n" lower case for angular
    public string Username { get; set; }

    // auto call method "GetAge()" in AppUser class

    public string PhotoUrl { get; set; }
    public int Age { get; set; }
    public string KnownAs { get; set; }
    public DateTime Created { get; set; }
    public DateTime LastActive { get; set; }
    public string Gender { get; set; }
    public string Introduction { get; set; }
    public string LookingFor { get; set; }
    public string Interests { get; set; }
    public string City { get; set; }
    public string Country { get; set; }

    // to avoid object cycle, we use this memberDto containing PhotoDto
    // in PhotoDto, not referencing to user object such that there is no cycle
    public ICollection<PhotoDto> Photos { get; set; }
  }
}