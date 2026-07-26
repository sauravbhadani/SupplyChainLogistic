using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderPilot.Api.Domain.Entities;
using OrderPilot.Api.Dtos.Admin;
using OrderPilot.Api.Services;

namespace OrderPilot.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/audit-logs")]
[Authorize(Roles = "Admin")]
public class AdminAuditLogsController : ControllerBase
{
    private readonly IAdminConfigService _adminConfigService;

    public AdminAuditLogsController(IAdminConfigService adminConfigService)
    {
        _adminConfigService = adminConfigService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuditLogDto>>> GetAll()
    {
        var logs = await _adminConfigService.GetAuditLogsAsync();
        return Ok(logs.Select(ToDto));
    }

    private static AuditLogDto ToDto(AuditLog log) => new()
    {
        Id = log.Id,
        UserId = log.UserId,
        Action = log.Action,
        EntityType = log.EntityType,
        EntityId = log.EntityId,
        Details = log.Details,
        TimestampUtc = log.TimestampUtc
    };
}
