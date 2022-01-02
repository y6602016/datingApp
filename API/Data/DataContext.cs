using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
  // ORM: Entity Framework => need DbContext to work
  // DbContext acts as a brdige between our entity classes and the database
  // we create DataContext implementing DbContext as the DbContext
  // Dbcontext will interact with DB
  public class DataContext : DbContext
  {
    public DataContext(DbContextOptions options) : base(options)
    {
    }

    // declare a method in DbContext class to operate entity class, we call it to interact with DB
    // it will return a set implementing AppUser class that we define in entity folder
    // DbContext.User.Add(new User{Id = 4, Name = John}) = INSERT INTO User(Id, Name) VALUES(4, John)
    public DbSet<AppUser> Users { get; set; }
  }
}