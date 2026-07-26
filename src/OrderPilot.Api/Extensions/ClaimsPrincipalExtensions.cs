using System.Security.Claims;

namespace OrderPilot.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal) =>
        Guid.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);
}
