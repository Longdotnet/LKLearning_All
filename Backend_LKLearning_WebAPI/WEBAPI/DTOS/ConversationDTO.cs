namespace WEBAPI.DTOS
{
    public class ConversationDTO
    {
        public int ConversationID { get; set; }
        public int AdminID { get; set; }
        public string AdminName { get; set; } // Tên admin
        public int UserID { get; set; }
        public string UserName { get; set; } // Tên user
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
