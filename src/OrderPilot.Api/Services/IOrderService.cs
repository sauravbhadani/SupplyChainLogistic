using OrderPilot.Api.Domain.Entities;

namespace OrderPilot.Api.Services;

public class InactiveCustomerException : Exception
{
}

public class NoActiveSupplierException : Exception
{
}

public class InvalidStatusTransitionException : Exception
{
    public InvalidStatusTransitionException(string message) : base(message)
    {
    }
}

public interface IOrderService
{
    Task<Order> CreateOrderAsync(Guid customerId, string productDescription, int quantity, string? notes);

    Task<List<Order>> GetOrdersForCustomerAsync(Guid customerId);

    Task<Order?> GetOrderByIdAsync(Guid orderId);

    Task<List<Order>> GetAllOrdersAsync(Guid? customerId, OrderStatus? status);

    Task<Order> UpdateOrderStatusAsync(Guid orderId, Guid actingUserId, OrderStatus newStatus);
}
