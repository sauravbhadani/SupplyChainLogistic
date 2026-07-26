using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderPilot.Api.Domain.Entities;
using OrderPilot.Api.Dtos.Admin;
using OrderPilot.Api.Extensions;
using OrderPilot.Api.Services;

namespace OrderPilot.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/orders")]
[Authorize(Roles = "Admin")]
public class AdminOrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public AdminOrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AdminOrderResponse>>> GetAll([FromQuery] Guid? customerId, [FromQuery] string? status)
    {
        OrderStatus? parsedStatus = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<OrderStatus>(status, ignoreCase: true, out var value))
            {
                return BadRequest(new { message = $"Unknown status '{status}'." });
            }
            parsedStatus = value;
        }

        var orders = await _orderService.GetAllOrdersAsync(customerId, parsedStatus);
        return Ok(orders.Select(ToResponse));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<AdminOrderResponse>> UpdateStatus(Guid id, UpdateOrderStatusRequest request)
    {
        if (!Enum.TryParse<OrderStatus>(request.Status, ignoreCase: true, out var newStatus))
        {
            return BadRequest(new { message = $"Unknown status '{request.Status}'." });
        }

        try
        {
            var order = await _orderService.UpdateOrderStatusAsync(id, User.GetUserId(), newStatus);
            return Ok(ToResponse(order));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidStatusTransitionException ex)
        {
            return Problem(title: ex.Message, statusCode: 409);
        }
    }

    private static AdminOrderResponse ToResponse(Order order) => new()
    {
        Id = order.Id,
        CustomerId = order.CustomerId,
        SupplierId = order.SupplierId,
        SupplierName = order.Supplier?.Name ?? string.Empty,
        OrderType = order.OrderType,
        ProductDescription = order.ProductDescription,
        Quantity = order.Quantity,
        Notes = order.Notes,
        Status = order.Status.ToString(),
        CreatedAtUtc = order.CreatedAtUtc,
        UpdatedAtUtc = order.UpdatedAtUtc
    };
}
