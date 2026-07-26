namespace OrderPilot.Api.Dtos.Admin;

public class CustomerDto
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string? CompanyName { get; set; }

    public bool IsPilotActive { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
