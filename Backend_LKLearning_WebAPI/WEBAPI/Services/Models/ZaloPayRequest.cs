using System.ComponentModel.DataAnnotations;

namespace WEBAPI.Services.Models;

public class ZaloPayRequest
{
    [Required]
    public decimal Amount { get; set; }

    [Required]
    public string Description { get; set; } = null!;

    [Required]
    public string OrderId { get; set; } = null!;

    [Required]
    public int UserId { get; set; }

    [Required]
    public int PackageId { get; set; }
}