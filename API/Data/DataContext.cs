using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
  // ORM: Entity Framework => need DbContext
  // DbContext acts as a brdige between out entity classes and the database
  // we create DataContext implementing DbContext as the DbContext
  public class DataContext : DbContext
  {
    public DataContext(DbContextOptions options) : base(options)
    {
    }

    // declare our entity class in DbContext class, we call it to interact with DB
    // DbContext.User.Add(new User{Id = 4, Name = John}) = INSERT INTO User(Id, Name) VALUES(4, John)
    public DbSet<AppUser> Users { get; set; }
  }
}