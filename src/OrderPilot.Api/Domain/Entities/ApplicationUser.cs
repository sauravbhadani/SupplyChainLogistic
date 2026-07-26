using Microsoft.AspNetCore.Identity;

namespace OrderPilot.Api.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? CompanyName { get; set; }

    public bool IsPilotActive { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
