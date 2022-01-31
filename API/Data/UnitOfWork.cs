using API.Interfaces;
using AutoMapper;

namespace API.Data
{
  public class UnitOfWork : IUnitOfWork
  {
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    // pass in context and mapper to create instance
    public UnitOfWork(DataContext context, IMapper mapper)
    {
      _mapper = mapper;
      _context = context;

    }

    // create instances of the repositories here
    public IUserRepository UserRepository => new UserRepository(_context, _mapper);

    public IMessageRepository MessageRepository => new MessageRepository(_context, _mapper);

    public ILikesRepository LikesRepository => new LikesRepository(_context);
    public async Task<bool> Complete()
    {
      return await _context.SaveChangesAsync() > 0;
    }

    public bool HasChanges()
    {
      return _context.ChangeTracker.HasChanges();
    }
  }
}