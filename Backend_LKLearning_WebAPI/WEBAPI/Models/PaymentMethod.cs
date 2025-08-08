using System;
using System.Collections.Generic;

namespace WEBAPI.Models;

public partial class PaymentMethod
{
    public int PaymentMethodId { get; set; }

    public string Name { get; set; } = null!;

    public string? Logo { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
