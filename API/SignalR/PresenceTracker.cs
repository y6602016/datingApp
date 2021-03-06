namespace API.SignalR
{
  public class PresenceTracker
  {
    // the OnlineUsers dict is a shared dict to record all user's connection record
    // we need to keep in mind that it can't be applied on scaled application since it's a shared dict
    private static readonly Dictionary<string, List<string>> OnlineUsers =
        new Dictionary<string, List<string>>();

    public Task<bool> UserConnected(string username, string connectionId)
    {
      bool isOnline = false;

      // since list string is a shared list, we need to lock OnlineUsers to prevent
      // concurrent user to access the dict at the same time
      lock (OnlineUsers)
      {
        // if the username key is alreay in the dict, then we add it's connectionId as value 
        if (OnlineUsers.ContainsKey(username))
        {
          OnlineUsers[username].Add(connectionId);
        }
        else
        { // otherwise we add a new list initialized with the connectionId
          OnlineUsers.Add(username, new List<string> { connectionId });
          isOnline = true;
        }
      }
      return Task.FromResult(isOnline);
    }

    public Task<bool> UserDisconnected(string username, string connectionId)
    {
      lock (OnlineUsers)
      {
        bool isOffline = false;
        // if not cantains the username, just return 
        if (!OnlineUsers.ContainsKey(username))
        {
          return Task.FromResult(isOffline);
        }
        // remove the connectionId from the connectionId list
        OnlineUsers[username].Remove(connectionId);

        // if connectionId list is empty, remove it from the dict
        if (OnlineUsers[username].Count == 0)
        {
          OnlineUsers.Remove(username);
          isOffline = true;
        }

        return Task.FromResult(isOffline);
      }
    }

    public Task<string[]> GetOnlineUsers()
    {
      string[] onlineUsers;
      lock (OnlineUsers)
      {
        // we order the dict by the key, then we select the key(username) only since we don't need value(connectionId)
        // then convert to array
        onlineUsers = OnlineUsers.OrderBy(k => k.Key).Select(k => k.Key).ToArray();
      }

      return Task.FromResult(onlineUsers);
    }


    public Task<List<string>> GetConnectionsForUser(string username)
    {
      List<string> connectionIds;
      lock (OnlineUsers)
      {
        connectionIds = OnlineUsers.GetValueOrDefault(username);
      }
      return Task.FromResult(connectionIds);
    }
  }
}