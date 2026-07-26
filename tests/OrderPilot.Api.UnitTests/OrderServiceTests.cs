using OrderPilot.Api.Domain.Entities;
using OrderPilot.Api.Services;
using Xunit;

namespace OrderPilot.Api.UnitTests;

public class OrderServiceTests
{
    private static ApplicationUser MakeCustomer(bool isPilotActive = true) => new()
    {
        Id = Guid.NewGuid(),
        UserName = "customer@example.com",
        Email = "customer@example.com",
        IsPilotActive = isPilotActive,
        CreatedAtUtc = DateTime.UtcNow
    };

    private static Supplier MakeSupplier(bool isActive = true) => new()
    {
        Id = Guid.NewGuid(),
        Name = "Acme Foods",
        Code = "ACME",
        IsActive = isActive,
        CreatedAtUtc = DateTime.UtcNow,
        UpdatedAtUtc = DateTime.UtcNow
    };

    [Fact]
    public async Task CreateOrderAsync_InactiveCustomer_ThrowsInactiveCustomerException()
    {
        using var context = TestDbContextFactory.Create();
        var customer = MakeCustomer(isPilotActive: false);
        context.Users.Add(customer);
        context.Suppliers.Add(MakeSupplier());
        await context.SaveChangesAsync();

        var service = new OrderService(context, new AuditService(context));

        await Assert.ThrowsAsync<InactiveCustomerException>(
            () => service.CreateOrderAsync(customer.Id, "Widgets", 10, null));
    }

    [Fact]
    public async Task CreateOrderAsync_NoActiveSupplier_ThrowsNoActiveSupplierException()
    {
        using var context = TestDbContextFactory.Create();
        var customer = MakeCustomer();
        context.Users.Add(customer);
        await context.SaveChangesAsync();

        var service = new OrderService(context, new AuditService(context));

        await Assert.ThrowsAsync<NoActiveSupplierException>(
            () => service.CreateOrderAsync(customer.Id, "Widgets", 10, null));
    }

    [Fact]
    public async Task CreateOrderAsync_ValidCustomerAndSupplier_CreatesOrderAndAuditLog()
    {
        using var context = TestDbContextFactory.Create();
        var customer = MakeCustomer();
        var supplier = MakeSupplier();
        context.Users.Add(customer);
        context.Suppliers.Add(supplier);
        await context.SaveChangesAsync();

        var service = new OrderService(context, new AuditService(context));

        var order = await service.CreateOrderAsync(customer.Id, "Widgets", 10, "Rush order");

        Assert.Equal(OrderStatus.Submitted, order.Status);
        Assert.Equal(supplier.Id, order.SupplierId);
        Assert.Equal(OrderTypes.Standard, order.OrderType);
        Assert.Single(context.AuditLogs);
        Assert.Equal(AuditActions.OrderCreated, context.AuditLogs.Single().Action);
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_NextSequentialStatus_Succeeds()
    {
        using var context = TestDbContextFactory.Create();
        var admin = Guid.NewGuid();
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            SupplierId = Guid.NewGuid(),
            ProductDescription = "Widgets",
            Quantity = 1,
            Status = OrderStatus.Submitted
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var service = new OrderService(context, new AuditService(context));

        var updated = await service.UpdateOrderStatusAsync(order.Id, admin, OrderStatus.Accepted);

        Assert.Equal(OrderStatus.Accepted, updated.Status);
        Assert.Single(context.AuditLogs);
        Assert.Contains("Submitted -> Accepted", context.AuditLogs.Single().Details);
    }

    [Theory]
    [InlineData(OrderStatus.Submitted, OrderStatus.Fulfilled)] // skip
    [InlineData(OrderStatus.Accepted, OrderStatus.Submitted)]  // backward
    public async Task UpdateOrderStatusAsync_IllegalTransition_ThrowsInvalidStatusTransitionException(
        OrderStatus current, OrderStatus attempted)
    {
        using var context = TestDbContextFactory.Create();
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            SupplierId = Guid.NewGuid(),
            ProductDescription = "Widgets",
            Quantity = 1,
            Status = current
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var service = new OrderService(context, new AuditService(context));

        await Assert.ThrowsAsync<InvalidStatusTransitionException>(
            () => service.UpdateOrderStatusAsync(order.Id, Guid.NewGuid(), attempted));
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_UnknownOrder_ThrowsKeyNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var service = new OrderService(context, new AuditService(context));

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.UpdateOrderStatusAsync(Guid.NewGuid(), Guid.NewGuid(), OrderStatus.Accepted));
    }
}
