using System.ComponentModel.DataAnnotations;

namespace OrderPilot.Api.Dtos.Orders;

public class CreateOrderRequest
{
    [Required, StringLength(500, MinimumLength = 1)]
    public string ProductDescription { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}
