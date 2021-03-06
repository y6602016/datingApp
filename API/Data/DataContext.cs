using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace API.Data
{
  // ORM: Entity Framework => need DbContext to work
  // DbContext acts as a brdige between our entity classes and the database
  // we create DataContext implementing DbContext as the DbContext
  // Dbcontext will interact with DB
  public class DataContext : IdentityDbContext<AppUser, AppRole, int,
    IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>,
    IdentityRoleClaim<int>, IdentityUserToken<int>>
  {
    public DataContext(DbContextOptions options) : base(options)
    {
      AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    // declare a method in DbContext class to operate entity class, we call it to interact with DB
    // it will return a set implementing AppUser class that we define in entity folder
    // DbContext.User.Add(new User{Id = 4, Name = John}) = INSERT INTO User(Id, Name) VALUES(4, John)

    // public DbSet<AppUser> Users { get; set; }

    public DbSet<UserLike> Likes { get; set; }
    public DbSet<Message> Messages { get; set; }

    public DbSet<Group> Groups { get; set; }
    public DbSet<Connection> Connections { get; set; }

    // override OnModelCreating which is in DbContext class
    protected override void OnModelCreating(ModelBuilder builder)
    {
      base.OnModelCreating(builder);


      // configuration for appUser and appRole, many to many
      builder.Entity<AppUser>()
        .HasMany(ur => ur.UserRoles)
        .WithOne(u => u.User)
        .HasForeignKey(ur => ur.UserId)
        .IsRequired();

      builder.Entity<AppRole>()
        .HasMany(ur => ur.UserRoles)
        .WithOne(u => u.Role)
        .HasForeignKey(ur => ur.RoleId)
        .IsRequired();

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

      builder.ApplyUtcDateTimeConverter();
    }
  }

  public static class UtcDateAnnotation
  {
    private const String IsUtcAnnotation = "IsUtc";
    private static readonly ValueConverter<DateTime, DateTime> UtcConverter =
      new ValueConverter<DateTime, DateTime>(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

    private static readonly ValueConverter<DateTime?, DateTime?> UtcNullableConverter =
      new ValueConverter<DateTime?, DateTime?>(v => v, v => v == null ? v : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc));

    public static PropertyBuilder<TProperty> IsUtc<TProperty>(this PropertyBuilder<TProperty> builder, Boolean isUtc = true) =>
      builder.HasAnnotation(IsUtcAnnotation, isUtc);

    public static Boolean IsUtc(this IMutableProperty property) =>
      ((Boolean?)property.FindAnnotation(IsUtcAnnotation)?.Value) ?? true;

    /// <summary>
    /// Make sure this is called after configuring all your entities.
    /// </summary>
    public static void ApplyUtcDateTimeConverter(this ModelBuilder builder)
    {
      foreach (var entityType in builder.Model.GetEntityTypes())
      {
        foreach (var property in entityType.GetProperties())
        {
          if (!property.IsUtc())
          {
            continue;
          }

          if (property.ClrType == typeof(DateTime))
          {
            property.SetValueConverter(UtcConverter);
          }

          if (property.ClrType == typeof(DateTime?))
          {
            property.SetValueConverter(UtcNullableConverter);
          }
        }
      }
    }
  }
}