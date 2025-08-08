using System;
using System.Collections.Generic;

namespace WEBAPI.Models;

public partial class Message
{
    public int MessageID { get; set; }

    public int ConversationID { get; set; }

    public int SenderID { get; set; }

    public string Content { get; set; } = null!;

    public DateTime SentAt { get; set; }

    public bool IsRead { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;

    public virtual User Sender { get; set; } = null!;
}
