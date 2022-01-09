using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
  public class UserRepository : IUserRepository
  {
    private readonly DataContext _context;
    public UserRepository(DataContext context)
    {
      _context = context;

    }

    public async Task<AppUser> GetUserAsync(int id)
    {
      return await _context.Users.FindAsync(id);
    }

    public async Task<AppUser> GetUserByUsernameAsync(string username)
    {
      return await _context.Users
      .Include(p => p.Photos)
      .SingleOrDefaultAsync(x => x.UserName == username);
    }

    public async Task<IEnumerable<AppUser>> GetUsersAsync()
    {
      // use eager lodaing to make user objects contain photos
      return await _context.Users
        .Include(p => p.Photos)
        .ToListAsync();
    }

    public async Task<bool> SaveAllAsync()
    {
      // make sure greater tham 0 chage has been saved
      // SaveChangesAsync() return int, so we return it's result > 0 or not
      return await _context.SaveChangesAsync() > 0;
    }

    public void Update(AppUser user)
    {
      // mark the entity has been modified
      _context.Entry(user).State = EntityState.Modified;
    }
  }
}