using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderPilot.Api.Domain.Entities;
using OrderPilot.Api.Dtos.Admin;
using OrderPilot.Api.Services;

namespace OrderPilot.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/suppliers")]
[Authorize(Roles = "Admin")]
public class AdminSuppliersController : ControllerBase
{
    private readonly IAdminConfigService _adminConfigService;

    public AdminSuppliersController(IAdminConfigService adminConfigService)
    {
        _adminConfigService = adminConfigService;
    }

    [HttpGet]
    public async Task<ActionResult<List<SupplierDto>>> GetAll()
    {
        var suppliers = await _adminConfigService.GetSuppliersAsync();
        return Ok(suppliers.Select(ToDto));
    }

    [HttpPost]
    public async Task<ActionResult<SupplierDto>> Create(CreateSupplierRequest request)
    {
        try
        {
            var supplier = await _adminConfigService.CreateSupplierAsync(request.Name, request.Code, request.IsActive);
            return CreatedAtAction(nameof(GetAll), ToDto(supplier));
        }
        catch (DuplicateSupplierCodeException)
        {
            return Problem(title: $"Supplier code '{request.Code}' is already in use.", statusCode: 409);
        }
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<SupplierDto>> Update(Guid id, UpdateSupplierRequest request)
    {
        try
        {
            var supplier = await _adminConfigService.UpdateSupplierAsync(id, request.IsActive, request.Name);
            return Ok(ToDto(supplier));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    private static SupplierDto ToDto(Supplier supplier) => new()
    {
        Id = supplier.Id,
        Name = supplier.Name,
        Code = supplier.Code,
        IsActive = supplier.IsActive,
        CreatedAtUtc = supplier.CreatedAtUtc,
        UpdatedAtUtc = supplier.UpdatedAtUtc
    };
}
