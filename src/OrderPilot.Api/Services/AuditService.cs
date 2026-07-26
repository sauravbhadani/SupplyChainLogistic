using OrderPilot.Api.Data;
using OrderPilot.Api.Domain.Entities;

namespace OrderPilot.Api.Services;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;

    public AuditService(ApplicationDbContext context)
    {
        _context = context;
    }

    public void Log(Guid userId, string action, string entityType, Guid entityId, string details)
    {
        _context.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            TimestampUtc = DateTime.UtcNow
        });
    }
}
