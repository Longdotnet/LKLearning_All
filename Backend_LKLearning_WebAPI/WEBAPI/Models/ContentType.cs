using System;
using System.Collections.Generic;

namespace WEBAPI.Models;

public partial class ContentType
{
    public int ContentTypeId { get; set; }

    public string TypeName { get; set; } = null!;

    public string? TypeDescription { get; set; }

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
