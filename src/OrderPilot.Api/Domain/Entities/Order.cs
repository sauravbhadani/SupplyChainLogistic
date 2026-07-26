namespace OrderPilot.Api.Domain.Entities;

public static class OrderTypes
{
    public const string Standard = "Standard";
}

public class Order
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    public ApplicationUser? Customer { get; set; }

    public Guid SupplierId { get; set; }

    public Supplier? Supplier { get; set; }

    public string OrderType { get; set; } = OrderTypes.Standard;

    public string ProductDescription { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public string? Notes { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Submitted;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
