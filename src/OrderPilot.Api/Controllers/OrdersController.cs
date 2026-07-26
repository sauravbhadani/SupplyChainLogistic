using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderPilot.Api.Authorization;
using OrderPilot.Api.Domain.Entities;
using OrderPilot.Api.Dtos.Orders;
using OrderPilot.Api.Extensions;
using OrderPilot.Api.Services;

namespace OrderPilot.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize(Roles = "Customer")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IAuthorizationService _authorizationService;

    public OrdersController(IOrderService orderService, IAuthorizationService authorizationService)
    {
        _orderService = orderService;
        _authorizationService = authorizationService;
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> Create(CreateOrderRequest request)
    {
        try
        {
            var order = await _orderService.CreateOrderAsync(User.GetUserId(), request.ProductDescription, request.Quantity, request.Notes);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, ToResponse(order));
        }
        catch (InactiveCustomerException)
        {
            return Forbid();
        }
        catch (NoActiveSupplierException)
        {
            return Problem(title: "No active supplier is configured for the pilot.", statusCode: 409);
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<OrderResponse>>> GetMine()
    {
        var orders = await _orderService.GetOrdersForCustomerAsync(User.GetUserId());
        return Ok(orders.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderResponse>> GetById(Guid id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order is null)
        {
            return NotFound();
        }

        var authResult = await _authorizationService.AuthorizeAsync(User, order, Policies.OrderOwnerOrAdmin);
        if (!authResult.Succeeded)
        {
            // 404, not 403 — avoids confirming existence of another customer's order.
            return NotFound();
        }

        return Ok(ToResponse(order));
    }

    private static OrderResponse ToResponse(Order order) => new()
    {
        Id = order.Id,
        OrderType = order.OrderType,
        ProductDescription = order.ProductDescription,
        Quantity = order.Quantity,
        Notes = order.Notes,
        Status = order.Status.ToString(),
        SupplierId = order.SupplierId,
        SupplierName = order.Supplier?.Name ?? string.Empty,
        CreatedAtUtc = order.CreatedAtUtc,
        UpdatedAtUtc = order.UpdatedAtUtc
    };
}
