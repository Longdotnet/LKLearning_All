using System.Collections.Concurrent;
namespace WEBAPI.Services
{
public class OnlineUserTracker : IOnlineUserTracker
    {
        private readonly ConcurrentDictionary<int, string> _onlineUsers = new();

        public void SetOnline(int userId, string connectionId) => _onlineUsers[userId] = connectionId;
        public void SetOffline(int userId) => _onlineUsers.TryRemove(userId, out _);
        public bool IsOnline(int userId) => _onlineUsers.ContainsKey(userId);
        public List<int> GetOnlineAdmins(List<int> adminIds) => adminIds.Where(id => _onlineUsers.ContainsKey(id)).ToList();
        public List<int> GetOnlineUsers(List<int> userIds) => userIds.Where(id => _onlineUsers.ContainsKey(id)).ToList();
    }

}
