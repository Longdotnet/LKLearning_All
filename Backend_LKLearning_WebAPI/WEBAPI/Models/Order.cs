using System;
using System.Collections.Generic;

namespace WEBAPI.Models;

public partial class Order
{
    public string OrderId { get; set; } = null!;

    public string? TransactionId { get; set; }

    public string Status { get; set; } = null!;

    public int UserId { get; set; }

    public int PackageId { get; set; }

    public DateTime CreatedAt { get; set; }

    public int PaymentMethodId { get; set; }

    public decimal Amount { get; set; }

    public virtual Package Package { get; set; } = null!;

    public virtual PaymentMethod PaymentMethod { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
public class UpdateOrderStatusDTO
{
    public string AppTransId { get; set; }
    public string Status { get; set; }
}
