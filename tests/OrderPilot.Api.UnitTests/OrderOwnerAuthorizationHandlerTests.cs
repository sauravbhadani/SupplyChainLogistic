using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using OrderPilot.Api.Authorization;
using OrderPilot.Api.Domain.Entities;
using Xunit;

namespace OrderPilot.Api.UnitTests;

public class OrderOwnerAuthorizationHandlerTests
{
    private static ClaimsPrincipal MakePrincipal(Guid userId, string role)
    {
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        ], "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    private static async Task<bool> RunHandlerAsync(ClaimsPrincipal user, Order resource)
    {
        var handler = new OrderOwnerAuthorizationHandler();
        var requirement = new OrderOwnerRequirement();
        var context = new AuthorizationHandlerContext([requirement], user, resource);

        await handler.HandleAsync(context);
        return context.HasSucceeded;
    }

    [Fact]
    public async Task Admin_AlwaysSucceeds_RegardlessOfOrderOwner()
    {
        var order = new Order { Id = Guid.NewGuid(), CustomerId = Guid.NewGuid() };
        var admin = MakePrincipal(Guid.NewGuid(), "Admin");

        Assert.True(await RunHandlerAsync(admin, order));
    }

    [Fact]
    public async Task MatchingCustomer_Succeeds()
    {
        var customerId = Guid.NewGuid();
        var order = new Order { Id = Guid.NewGuid(), CustomerId = customerId };
        var customer = MakePrincipal(customerId, "Customer");

        Assert.True(await RunHandlerAsync(customer, order));
    }

    [Fact]
    public async Task DifferentCustomer_DoesNotSucceed()
    {
        var order = new Order { Id = Guid.NewGuid(), CustomerId = Guid.NewGuid() };
        var otherCustomer = MakePrincipal(Guid.NewGuid(), "Customer");

        Assert.False(await RunHandlerAsync(otherCustomer, order));
    }
}
