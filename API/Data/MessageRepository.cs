using API.DTOs;
using API.Entities;
using API.Extensions;
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

    public void AddGroup(Group group)
    {
      _context.Groups.Add(group);
    }

    public void AddMessage(Message message)
    {
      _context.Messages.Add(message);
    }

    public void DeleteMessage(Message message)
    {
      _context.Messages.Remove(message);
    }

    public async Task<Connection> GetConnection(string connectionId)
    {
      return await _context.Connections.FindAsync(connectionId);
    }

    public async Task<Group> GetGroupForConnection(string connectionId)
    {
      return await _context.Groups
        .Include(c => c.Connections)
        .Where(c => c.Connections.Any(x => x.ConnectionId == connectionId))
        .FirstOrDefaultAsync();
    }

    public async Task<Message> GetMessage(int id)
    {
      return await _context.Messages
        .Include(u => u.Sender)
        .Include(u => u.Recipient)
        .SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Group> GetMessageGroup(string groupName)
    {
      return await _context.Groups
        .Include(x => x.Connections)
        .FirstOrDefaultAsync(x => x.Name == groupName);
    }

    public async Task<PageList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
    {
      // convert the all messages as Queryable
      var query = _context.Messages
        .OrderByDescending(m => m.MessageSent)
        .ProjectTo<MessageDto>(_mapper.ConfigurationProvider) // project to Dto here, then no need to use too much "select" below
        .AsQueryable();

      // filter out the needed query objects
      query = messageParams.Container switch
      {
        "Inbox" => query.Where(u => u.RecipientUsername == messageParams.Username
          && u.RecipientDeleted == false),
        "Outbox" => query.Where(u => u.SenderUsername == messageParams.Username
          && u.SenderDeleted == false),
        _ => query.Where(u => u.RecipientUsername == messageParams.Username
          && u.RecipientDeleted == false
          && u.DateRead == null) // default case, not read yet
      };

      // project the query to dto, not do it here, do it earlier in the query creation
      // var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

      return await PageList<MessageDto>.CreateAsync(query, messageParams.PageNumber, messageParams.PageSize);
    }

    // get all messages between currentUser and recipientUser no matter who sends
    public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
    {
      var messages = await _context.Messages
        // .Include(u => u.Sender).ThenInclude(p => p.Photos) // no need anymore since we use projection below, "Include" uses "join"
        // .Include(u => u.Recipient).ThenInclude(p => p.Photos)
        .Where(m => ((m.Recipient.UserName == currentUsername && m.RecipientDeleted == false)
          && m.Sender.UserName == recipientUsername)
          || (m.Recipient.UserName == recipientUsername
          && (m.Sender.UserName == currentUsername && m.SenderDeleted == false))
        )
        .MarkUnreadAsRead(currentUsername) // fix EF change tracking bug. because we use projection below, we need to modify entity
                                           // before we project them. so we get unread entity and mark them first, process it in the MarkUnreadAsRead function in QueryableExtensions file
        .OrderBy(m => m.MessageSent)
        .ProjectTo<MessageDto>(_mapper.ConfigurationProvider) // project to Dto here instead doing it with mapper.Map at the end, 
                                                              // ProjectTo must be the last call in the chain. ORMs work with entities, not DTOs. 
                                                              // So apply any filtering and sorting on entities and, as the last step, project to DTOs.
        .ToListAsync();

      // return _mapper.Map<IEnumerable<MessageDto>>(messages);
      return messages;
    }

    public void RemoveConnection(Connection connection)
    {
      _context.Connections.Remove(connection);
    }

    // public async Task<bool> SavaAllAsync()
    // {
    //   return await _context.SaveChangesAsync() > 0;
    // }
  }
}