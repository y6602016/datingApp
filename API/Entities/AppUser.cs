using Microsoft.AspNetCore.Identity;

namespace API.Entities
{
  public class AppUser : IdentityUser<int>
  {
    // we've used IdentityUser, no need to defind these by ourselve
    // public int Id { get; set; }
    // public string UserName { get; set; }
    // public byte[] PasswordHash { get; set; }
    // public byte[] PasswordSalt { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string KnownAs { get; set; }
    public DateTime Created { get; set; } = DateTime.Now;
    public DateTime LastActive { get; set; } = DateTime.Now;
    public string Gender { get; set; }
    public string Introduction { get; set; }
    public string LookingFor { get; set; }
    public string Interests { get; set; }
    public string City { get; set; }
    public string Country { get; set; }

    // photo doesn't need to be DbSet in DataContext since we won't use photo directly
    // photo has dependency on user, we can create a table for photo
    // entityframwork is smart enough to know user has a photo foreign key linking
    // to the photo table since we declare photo in here
    // but we'd like to use entity frameword convention to define onDelete cascade
    // so we use fully defining relationship, which declare user in photo as well
    public ICollection<Photo> Photos { get; set; }

    public ICollection<UserLike> LikedByUsers { get; set; }

    public ICollection<UserLike> LikedUsers { get; set; }


    public ICollection<Message> MessagesSent { get; set; }
    public ICollection<Message> MessagesReceived { get; set; }
    public ICollection<AppUserRole> UserRoles { get; set; }

    // although GetAge can be used for memberDto Age prop, but it leads that automapper QueryableExtensions
    // map process will select all properties including passwordhash and passwordsalt
    // we can get age property in AutoMapperProfiles.cs, which is the Configuration file

    // public int GetAge()
    // {
    //   return DataOfBirth.CaculateAge();
    // }
  }
}