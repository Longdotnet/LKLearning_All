namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Models
{
    public class COnlineUserInfo
    {
        public int UserId { get; set; }
        public string Fullname { get; set; }
    }


    public class COnlineUsersAndAdminsResult
    {
        public List<COnlineUserInfo> OnlineAdmins { get; set; } = new();
        public List<COnlineUserInfo> OnlineUsers { get; set; } = new();
    }
}