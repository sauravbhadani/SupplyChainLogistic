namespace OrderPilot.Api.Dtos.Orders;

public class OrderResponse
{
    public Guid Id { get; set; }

    public string OrderType { get; set; } = string.Empty;

    public string ProductDescription { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public string? Notes { get; set; }

    public string Status { get; set; } = string.Empty;

    public Guid SupplierId { get; set; }

    public string SupplierName { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
