using System;
using System.Collections.Generic;

namespace WEBAPI.Models;

public partial class PaymentMethodDTO
{
    public int PaymentMethodId { get; set; }

    public string Name { get; set; } = null!;

    public string? Logo { get; set; }

    
}
