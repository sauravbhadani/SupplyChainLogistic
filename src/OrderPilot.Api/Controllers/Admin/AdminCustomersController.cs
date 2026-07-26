using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderPilot.Api.Domain.Entities;
using OrderPilot.Api.Dtos.Admin;
using OrderPilot.Api.Services;

namespace OrderPilot.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/customers")]
[Authorize(Roles = "Admin")]
public class AdminCustomersController : ControllerBase
{
    private readonly IAdminConfigService _adminConfigService;

    public AdminCustomersController(IAdminConfigService adminConfigService)
    {
        _adminConfigService = adminConfigService;
    }

    [HttpGet]
    public async Task<ActionResult<List<CustomerDto>>> GetAll()
    {
        var customers = await _adminConfigService.GetCustomersAsync();
        return Ok(customers.Select(ToDto));
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create(CreateCustomerRequest request)
    {
        try
        {
            var customer = await _adminConfigService.CreateCustomerAsync(
                request.Email, request.Password, request.CompanyName, request.IsPilotActive);
            return CreatedAtAction(nameof(GetAll), ToDto(customer));
        }
        catch (InvalidOperationException ex)
        {
            return Problem(title: ex.Message, statusCode: 400);
        }
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<CustomerDto>> Update(Guid id, UpdateCustomerRequest request)
    {
        try
        {
            var customer = await _adminConfigService.UpdateCustomerAsync(id, request.IsPilotActive, request.CompanyName);
            return Ok(ToDto(customer));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    private static CustomerDto ToDto(ApplicationUser user) => new()
    {
        Id = user.Id,
        Email = user.Email ?? string.Empty,
        CompanyName = user.CompanyName,
        IsPilotActive = user.IsPilotActive,
        CreatedAtUtc = user.CreatedAtUtc
    };
}
