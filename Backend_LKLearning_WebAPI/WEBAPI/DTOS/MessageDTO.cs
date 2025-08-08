namespace WEBAPI.DTOS
{
    public class MessageDTO
    {
        public int MessageID { get; set; }
        public int ConversationID { get; set; }
        public int SenderID { get; set; }
        public string? SenderName { get; set; } // Tên người gửi
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
    }
}
