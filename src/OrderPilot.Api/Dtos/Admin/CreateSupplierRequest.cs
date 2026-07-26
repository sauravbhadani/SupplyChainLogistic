using System.ComponentModel.DataAnnotations;

namespace OrderPilot.Api.Dtos.Admin;

public class CreateSupplierRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(50, MinimumLength = 1)]
    public string Code { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}
