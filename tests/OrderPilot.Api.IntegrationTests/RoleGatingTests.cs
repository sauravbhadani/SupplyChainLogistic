using System.Net;
using Xunit;

namespace OrderPilot.Api.IntegrationTests;

public class RoleGatingTests
{
    [Theory]
    [InlineData("/api/admin/orders")]
    [InlineData("/api/admin/customers")]
    [InlineData("/api/admin/suppliers")]
    [InlineData("/api/admin/audit-logs")]
    public async Task Customer_HittingAdminRoute_Returns403(string route)
    {
        await using var factory = new CustomWebApplicationFactory();
        await factory.InitializeDatabaseAsync();
        await TestDataHelper.EnsureRolesAsync(factory);
        await TestDataHelper.CreateActiveSupplierAsync(factory, "ACME");
        await TestDataHelper.CreateCustomerAsync(factory, "customer@example.com");

        using var client = factory.CreateClient();
        var token = await TestDataHelper.LoginAsync(client, "customer@example.com");
        client.WithBearerToken(token);

        var response = await client.GetAsync(route);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_HittingCustomerOrdersRoute_Returns403()
    {
        await using var factory = new CustomWebApplicationFactory();
        await factory.InitializeDatabaseAsync();
        await TestDataHelper.EnsureRolesAsync(factory);
        await TestDataHelper.CreateAdminAsync(factory, "admin@example.com");

        using var client = factory.CreateClient();
        var token = await TestDataHelper.LoginAsync(client, "admin@example.com");
        client.WithBearerToken(token);

        var response = await client.GetAsync("/api/orders");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AnonymousRequest_ToProtectedRoute_Returns401()
    {
        await using var factory = new CustomWebApplicationFactory();
        await factory.InitializeDatabaseAsync();

        using var client = factory.CreateClient();
        var response = await client.GetAsync("/api/orders");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
