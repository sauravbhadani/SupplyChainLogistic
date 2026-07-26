using Microsoft.EntityFrameworkCore;
using OrderPilot.Api.Data;
using OrderPilot.Api.Domain.Entities;

namespace OrderPilot.Api.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public OrderService(ApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<Order> CreateOrderAsync(Guid customerId, string productDescription, int quantity, string? notes)
    {
        var customer = await _context.Users.SingleOrDefaultAsync(u => u.Id == customerId);
        if (customer is null || !customer.IsPilotActive)
        {
            throw new InactiveCustomerException();
        }

        var activeSupplier = await _context.Suppliers.SingleOrDefaultAsync(s => s.IsActive);
        if (activeSupplier is null)
        {
            throw new NoActiveSupplierException();
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            SupplierId = activeSupplier.Id,
            OrderType = OrderTypes.Standard,
            ProductDescription = productDescription,
            Quantity = quantity,
            Notes = notes,
            Status = OrderStatus.Submitted,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _context.Orders.Add(order);
        _auditService.Log(customerId, AuditActions.OrderCreated, "Order", order.Id,
            $"Order created for supplier '{activeSupplier.Name}'.");

        await _context.SaveChangesAsync();

        order.Supplier = activeSupplier;
        return order;
    }

    public async Task<List<Order>> GetOrdersForCustomerAsync(Guid customerId)
    {
        return await _context.Orders
            .Include(o => o.Supplier)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync();
    }

    public async Task<Order?> GetOrderByIdAsync(Guid orderId)
    {
        return await _context.Orders
            .Include(o => o.Supplier)
            .SingleOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<List<Order>> GetAllOrdersAsync(Guid? customerId, OrderStatus? status)
    {
        var query = _context.Orders.Include(o => o.Supplier).AsQueryable();

        if (customerId.HasValue)
        {
            query = query.Where(o => o.CustomerId == customerId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        return await query.OrderByDescending(o => o.CreatedAtUtc).ToListAsync();
    }

    public async Task<Order> UpdateOrderStatusAsync(Guid orderId, Guid actingUserId, OrderStatus newStatus)
    {
        var order = await _context.Orders.SingleOrDefaultAsync(o => o.Id == orderId)
            ?? throw new KeyNotFoundException($"Order '{orderId}' was not found.");

        if ((int)newStatus != (int)order.Status + 1)
        {
            throw new InvalidStatusTransitionException(
                $"Cannot transition order from '{order.Status}' to '{newStatus}'. Only the next sequential status is allowed.");
        }

        var previousStatus = order.Status;
        order.Status = newStatus;
        order.UpdatedAtUtc = DateTime.UtcNow;

        _auditService.Log(actingUserId, AuditActions.OrderStatusChanged, "Order", order.Id,
            $"{previousStatus} -> {newStatus}");

        await _context.SaveChangesAsync();

        return order;
    }
}
