namespace OrderPilot.Api.Dtos.Admin;

public class UpdateCustomerRequest
{
    public bool IsPilotActive { get; set; }

    public string? CompanyName { get; set; }
}
