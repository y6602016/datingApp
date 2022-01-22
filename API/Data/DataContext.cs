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

    public DbSet<UserLike> Likes { get; set; }
    public DbSet<Message> Messages { get; set; }

    // override OnModelCreating which is in DbContext class
    protected override void OnModelCreating(ModelBuilder builder)
    {
      base.OnModelCreating(builder);

      // pk is the combination of the source userId and liked userId
      // defind pk here
      builder.Entity<UserLike>()
        .HasKey(k => new { k.SourceUserId, k.LikedUserId });


      // table looks like:
      // sourceUserId      LikedUserId
      //      2                 4
      //      3                 1

      // a source user can have many liked user
      builder.Entity<UserLike>()
        .HasOne(s => s.SourceUser)
        .WithMany(l => l.LikedUsers)
        .HasForeignKey(s => s.SourceUserId)
        .OnDelete(DeleteBehavior.Cascade);


      // a liked user can have many likedBy user (or follower)
      builder.Entity<UserLike>()
        .HasOne(s => s.LikedUser)
        .WithMany(l => l.LikedByUsers)
        .HasForeignKey(s => s.LikedUserId)
        .OnDelete(DeleteBehavior.Cascade);

      builder.Entity<Message>()
        .HasOne(u => u.Recipient)
        .WithMany(m => m.MessagesReceived)
        .OnDelete(DeleteBehavior.Restrict);

      builder.Entity<Message>()
        .HasOne(u => u.Sender)
        .WithMany(m => m.MessagesSent)
        .OnDelete(DeleteBehavior.Restrict);
    }
  }
}