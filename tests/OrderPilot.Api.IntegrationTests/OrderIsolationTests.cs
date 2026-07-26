using System.Net;
using System.Net.Http.Json;
using OrderPilot.Api.Dtos.Orders;
using Xunit;

namespace OrderPilot.Api.IntegrationTests;

/// <summary>
/// Direct proof point for the feasibility report's HIGH-severity authorization risk:
/// customer A must never be able to see customer B's orders. Treat this as a required
/// CI gate before pilot go-live, not an optional nice-to-have.
/// </summary>
public class OrderIsolationTests
{
    [Fact]
    public async Task CustomerB_CannotGetCustomerA_OrderById()
    {
        await using var factory = new CustomWebApplicationFactory();
        await factory.InitializeDatabaseAsync();
        await TestDataHelper.EnsureRolesAsync(factory);
        await TestDataHelper.CreateActiveSupplierAsync(factory, "ACME");

        var customerA = await TestDataHelper.CreateCustomerAsync(factory, "a@example.com");
        await TestDataHelper.CreateCustomerAsync(factory, "b@example.com");
        var orderA = await TestDataHelper.CreateOrderDirectlyAsync(factory, customerA.Id);

        using var client = factory.CreateClient();
        var tokenB = await TestDataHelper.LoginAsync(client, "b@example.com");
        client.WithBearerToken(tokenB);

        var response = await client.GetAsync($"/api/orders/{orderA.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CustomerB_OrderList_NeverIncludesCustomerA_Order()
    {
        await using var factory = new CustomWebApplicationFactory();
        await factory.InitializeDatabaseAsync();
        await TestDataHelper.EnsureRolesAsync(factory);
        await TestDataHelper.CreateActiveSupplierAsync(factory, "ACME");

        var customerA = await TestDataHelper.CreateCustomerAsync(factory, "a@example.com");
        var customerB = await TestDataHelper.CreateCustomerAsync(factory, "b@example.com");
        var orderA = await TestDataHelper.CreateOrderDirectlyAsync(factory, customerA.Id, "Customer A's widgets");
        await TestDataHelper.CreateOrderDirectlyAsync(factory, customerB.Id, "Customer B's widgets");

        using var client = factory.CreateClient();
        var tokenB = await TestDataHelper.LoginAsync(client, "b@example.com");
        client.WithBearerToken(tokenB);

        var response = await client.GetAsync("/api/orders");
        response.EnsureSuccessStatusCode();
        var orders = await response.Content.ReadFromJsonAsync<List<OrderResponse>>();

        Assert.Single(orders!);
        Assert.DoesNotContain(orders!, o => o.Id == orderA.Id);
    }

    [Fact]
    public async Task CustomerA_CanGetOwnOrder()
    {
        await using var factory = new CustomWebApplicationFactory();
        await factory.InitializeDatabaseAsync();
        await TestDataHelper.EnsureRolesAsync(factory);
        await TestDataHelper.CreateActiveSupplierAsync(factory, "ACME");

        var customerA = await TestDataHelper.CreateCustomerAsync(factory, "a@example.com");
        var orderA = await TestDataHelper.CreateOrderDirectlyAsync(factory, customerA.Id);

        using var client = factory.CreateClient();
        var tokenA = await TestDataHelper.LoginAsync(client, "a@example.com");
        client.WithBearerToken(tokenA);

        var response = await client.GetAsync($"/api/orders/{orderA.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Admin_CanGetAnyCustomer_OrderById()
    {
        await using var factory = new CustomWebApplicationFactory();
        await factory.InitializeDatabaseAsync();
        await TestDataHelper.EnsureRolesAsync(factory);
        await TestDataHelper.CreateActiveSupplierAsync(factory, "ACME");
        await TestDataHelper.CreateAdminAsync(factory, "admin@example.com");

        var customerA = await TestDataHelper.CreateCustomerAsync(factory, "a@example.com");
        var orderA = await TestDataHelper.CreateOrderDirectlyAsync(factory, customerA.Id);

        using var client = factory.CreateClient();
        var adminToken = await TestDataHelper.LoginAsync(client, "admin@example.com");
        client.WithBearerToken(adminToken);

        var response = await client.GetAsync($"/api/admin/orders");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var orders = await response.Content.ReadFromJsonAsync<List<OrderPilot.Api.Dtos.Admin.AdminOrderResponse>>();
        Assert.Contains(orders!, o => o.Id == orderA.Id);
    }
}
