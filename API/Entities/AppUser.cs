// we define schema in entity class file
using API.Extensions;

namespace API.Entities
{
  public class AppUser
  {
    public int Id { get; set; }
    public string UserName { get; set; }
    public byte[] PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }
    public DateTime DataOfBirth { get; set; }
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

    public int GetAge()
    {
      return DataOfBirth.CaculateAge();
    }
  }
}