using System;
using System.Linq;
using API.Entities;

namespace API.Extensions
{
  public static class QueryableExtensions
  {
    // fix EF change tracking bug, we mark unread data's DateRead here to make sure EF track this change
    public static IQueryable<Message> MarkUnreadAsRead(this IQueryable<Message> query, string currentUsername)
    {
      var unreadMessages = query.Where(m => m.DateRead == null
          && m.RecipientUsername == currentUsername);

      if (unreadMessages.Any())
      {
        foreach (var message in unreadMessages)
        {
          message.DateRead = DateTime.UtcNow;
        }
      }

      return query;
    }
  }
}