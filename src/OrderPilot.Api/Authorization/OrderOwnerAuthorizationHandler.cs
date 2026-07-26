using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using OrderPilot.Api.Domain.Entities;

namespace OrderPilot.Api.Authorization;

public class OrderOwnerAuthorizationHandler : AuthorizationHandler<OrderOwnerRequirement, Order>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, OrderOwnerRequirement requirement, Order resource)
    {
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var userId) && userId == resource.CustomerId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
