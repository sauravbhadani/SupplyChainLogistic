using System.ComponentModel.DataAnnotations;

namespace OrderPilot.Api.Dtos.Admin;

public class UpdateOrderStatusRequest
{
    [Required]
    public string Status { get; set; } = string.Empty;
}
