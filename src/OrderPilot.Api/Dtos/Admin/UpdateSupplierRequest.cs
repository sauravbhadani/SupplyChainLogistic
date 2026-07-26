namespace OrderPilot.Api.Dtos.Admin;

public class UpdateSupplierRequest
{
    public bool IsActive { get; set; }

    public string? Name { get; set; }
}
