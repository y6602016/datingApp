using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
  public class MessageRepository : IMessageRepository
  {
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    public MessageRepository(DataContext context, IMapper mapper)
    {
      _mapper = mapper;
      _context = context;
    }

    public void AddMessage(Message message)
    {
      _context.Messages.Add(message);
    }

    public void DeleteMessage(Message message)
    {
      _context.Messages.Remove(message);
    }

    public async Task<Message> GetMessage(int id)
    {
      return await _context.Messages.FindAsync(id);
    }

    public async Task<PageList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
    {
      // convert the all messages as Queryable
      var query = _context.Messages
        .OrderByDescending(m => m.MessageSent)
        .AsQueryable();

      // filter out the needed query objects
      query = messageParams.Container switch
      {
        "Inbox" => query.Where(u => u.Recipient.UserName == messageParams.Username),
        "Outbox" => query.Where(u => u.Sender.UserName == messageParams.Username),
        _ => query.Where(u => u.Recipient.UserName ==
          messageParams.Username && u.DateRead == null) // default case, not read yet
      };

      // prohect the query to dto
      var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

      return await PageList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
    }

    // get all messages between currentUser and recipientUser no matter who sends
    public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
    {
      var messages = await _context.Messages
        .Include(u => u.Sender).ThenInclude(p => p.Photos)
        .Include(u => u.Recipient).ThenInclude(p => p.Photos)
        .Where(m => (m.Recipient.UserName == currentUsername
          && m.Sender.UserName == recipientUsername)
          || (m.Recipient.UserName == recipientUsername
          && m.Sender.UserName == currentUsername)
        )
        .OrderBy(m => m.MessageSent)
        .ToListAsync();

      var unreadMessages = messages.Where(m => m.DateRead == null && m.Recipient.UserName == currentUsername).ToList();
      if (unreadMessages.Any())
      {
        foreach (var message in unreadMessages)
        {
          message.DateRead = DateTime.Now;
        }
        await _context.SaveChangesAsync();
      }

      return _mapper.Map<IEnumerable<MessageDto>>(messages);
    }

    public async Task<bool> SavaAllAsync()
    {
      return await _context.SaveChangesAsync() > 0;
    }
  }
}