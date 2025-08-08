using System;
using System.Collections.Generic;

namespace WEBAPI.Models;

public partial class Avatar
{
    public int AvatarId { get; set; }

    public string UrlPath { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
