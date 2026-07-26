namespace OrderPilot.Api.Services;

public interface IAuditService
{
    /// <summary>
    /// Stages an audit log entry on the current DbContext without saving.
    /// Callers must call SaveChangesAsync() themselves so the audit row commits
    /// atomically alongside the state change it records.
    /// </summary>
    void Log(Guid userId, string action, string entityType, Guid entityId, string details);
}
