namespace WEBAPI.Services
{
    public interface IOnlineUserTracker
    {
        void SetOnline(int userId, string connectionId);
        void SetOffline(int userId);
        bool IsOnline(int userId);
        List<int> GetOnlineAdmins(List<int> adminIds);
        List<int> GetOnlineUsers(List<int> userIds);

    }
}
