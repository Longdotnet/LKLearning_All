using System;
using System.Collections.Generic;

namespace WEBAPI.Models;

public partial class Conversation
{
    public int ConversationID { get; set; }

    public int AdminID { get; set; }

    public int UserID { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; }

    public virtual User Admin { get; set; } = null!;

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual User User { get; set; } = null!;
}
