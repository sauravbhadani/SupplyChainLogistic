using System.ComponentModel.DataAnnotations;

namespace OrderPilot.Api.Dtos.Admin;

public class CreateCustomerRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [StringLength(200)]
    public string? CompanyName { get; set; }

    public bool IsPilotActive { get; set; } = true;
}
