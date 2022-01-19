using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
  public class UserRepository : IUserRepository
  {
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    public UserRepository(DataContext context, IMapper mapper)
    {
      _mapper = mapper;
      _context = context;

    }

    // <optimized>
    // we may not need to use repository in controller then map to member Dto
    // we may directly map objects from DbContext to Dto in repository here
    // we can use automapper QueryableExtensions to map them
    public async Task<MemberDto> GetMemberAsync(string username)
    {
      // if we don't use QueryableExtensions here, we need to map it to dto manually
      // .Where(x => x.UserName == username)
      // .Select(user => new MemerDto) {
      //    Id = user.Id
      //    Username = user.UserName.... 
      // }
      // .SingleOrDefaultAsync();

      // we can notice that we need to map all proeperties mannually, it's not acceptable
      // so we use ProjectTo. which is one of QueryableExtension method, with configuration file to map to memberDto
      return await _context.Users
        .Where(x => x.UserName == username)
        .ProjectTo<MemberDto>(_mapper.ConfigurationProvider) // ConfigurationProvider is AutoMapperProfiles.cs
        .SingleOrDefaultAsync();
    }

    // <optimized>
    public async Task<PageList<MemberDto>> GetMembersAsync(UserParams userParams)
    {
      var query = _context.Users.AsQueryable(); // make it be AsQueryable so it can be filtered by some criterias

      // filter: find out all other users and filter them by the opposite gender, which means
      // male user will receive femail user and female user get male users
      query = query.Where(u => u.UserName != userParams.CurrentUsername);
      query = query.Where(u => u.Gender == userParams.Gender);

      // pass the query as source and the pagenumber and pagesize, CreateAsync will return the 
      // PageList witg elements of this page
      return await PageList<MemberDto>.CreateAsync(query.ProjectTo<MemberDto>(_mapper.
        ConfigurationProvider).AsNoTracking(),  // the query is read-only, so we use no tracking
          userParams.PageNumber, userParams.PageSize);
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