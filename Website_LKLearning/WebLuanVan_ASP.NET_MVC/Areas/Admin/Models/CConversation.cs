namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Models
{
    public class CConversation
    {
        public int ConversationID { get; set; }
        public int AdminID { get; set; }
        public string AdminName { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
