using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using OrderPilot.Api.Data;
using OrderPilot.Api.Domain.Entities;
using OrderPilot.Api.Dtos.Admin;
using Xunit;

namespace OrderPilot.Api.IntegrationTests;

public class AdminOrderWorkflowTests
{
    [Fact]
    public async Task Admin_UpdatesOrderStatus_ProducesCorrectAuditRow()
    {
        await using var factory = new CustomWebApplicationFactory();
        await factory.InitializeDatabaseAsync();
        await TestDataHelper.EnsureRolesAsync(factory);
        await TestDataHelper.CreateActiveSupplierAsync(factory, "ACME");
        var admin = await TestDataHelper.CreateAdminAsync(factory, "admin@example.com");
        var customer = await TestDataHelper.CreateCustomerAsync(factory, "customer@example.com");
        var order = await TestDataHelper.CreateOrderDirectlyAsync(factory, customer.Id);

        using var client = factory.CreateClient();
        var token = await TestDataHelper.LoginAsync(client, "admin@example.com");
        client.WithBearerToken(token);

        var response = await client.PatchAsJsonAsync($"/api/admin/orders/{order.Id}/status", new UpdateOrderStatusRequest { Status = "Accepted" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var auditRows = context.AuditLogs.Where(a => a.EntityId == order.Id && a.Action == AuditActions.OrderStatusChanged).ToList();

        var auditRow = Assert.Single(auditRows);
        Assert.Equal(admin.Id, auditRow.UserId);
        Assert.Contains("Submitted -> Accepted", auditRow.Details);
    }

    [Fact]
    public async Task Admin_SkippingStatusTransition_Returns409()
    {
        await using var factory = new CustomWebApplicationFactory();
        await factory.InitializeDatabaseAsync();
        await TestDataHelper.EnsureRolesAsync(factory);
        await TestDataHelper.CreateActiveSupplierAsync(factory, "ACME");
        await TestDataHelper.CreateAdminAsync(factory, "admin@example.com");
        var customer = await TestDataHelper.CreateCustomerAsync(factory, "customer@example.com");
        var order = await TestDataHelper.CreateOrderDirectlyAsync(factory, customer.Id);

        using var client = factory.CreateClient();
        var token = await TestDataHelper.LoginAsync(client, "admin@example.com");
        client.WithBearerToken(token);

        var response = await client.PatchAsJsonAsync($"/api/admin/orders/{order.Id}/status", new UpdateOrderStatusRequest { Status = "Fulfilled" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Admin_ActivatingSupplier_DeactivatesPreviouslyActiveSupplier()
    {
        await using var factory = new CustomWebApplicationFactory();
        await factory.InitializeDatabaseAsync();
        await TestDataHelper.EnsureRolesAsync(factory);
        var original = await TestDataHelper.CreateActiveSupplierAsync(factory, "ORIG");
        await TestDataHelper.CreateAdminAsync(factory, "admin@example.com");

        using var client = factory.CreateClient();
        var token = await TestDataHelper.LoginAsync(client, "admin@example.com");
        client.WithBearerToken(token);

        var createResponse = await client.PostAsJsonAsync("/api/admin/suppliers",
            new CreateSupplierRequest { Name = "New Supplier", Code = "NEW", IsActive = true });
        createResponse.EnsureSuccessStatusCode();

        var getResponse = await client.GetAsync("/api/admin/suppliers");
        var suppliers = await getResponse.Content.ReadFromJsonAsync<List<SupplierDto>>();

        var reloadedOriginal = suppliers!.Single(s => s.Id == original.Id);
        var newSupplier = suppliers!.Single(s => s.Code == "NEW");
        Assert.False(reloadedOriginal.IsActive);
        Assert.True(newSupplier.IsActive);
    }

    [Fact]
    public async Task InactivePilotCustomer_CannotLogIn()
    {
        await using var factory = new CustomWebApplicationFactory();
        await factory.InitializeDatabaseAsync();
        await TestDataHelper.EnsureRolesAsync(factory);
        await TestDataHelper.CreateCustomerAsync(factory, "inactive@example.com", isPilotActive: false);

        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new OrderPilot.Api.Dtos.Auth.LoginRequest { Email = "inactive@example.com", Password = TestDataHelper.DefaultPassword });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_MalformedRequest_Returns400()
    {
        await using var factory = new CustomWebApplicationFactory();
        await factory.InitializeDatabaseAsync();
        await TestDataHelper.EnsureRolesAsync(factory);
        await TestDataHelper.CreateActiveSupplierAsync(factory, "ACME");
        await TestDataHelper.CreateCustomerAsync(factory, "customer@example.com");

        using var client = factory.CreateClient();
        var token = await TestDataHelper.LoginAsync(client, "customer@example.com");
        client.WithBearerToken(token);

        // Missing ProductDescription (required) and Quantity <= 0 (must be > 0)
        var response = await client.PostAsJsonAsync("/api/orders", new { productDescription = "", quantity = 0 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
