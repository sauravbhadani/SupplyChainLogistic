using OrderPilot.Api.Domain.Entities;
using OrderPilot.Api.Services;
using Xunit;

namespace OrderPilot.Api.UnitTests;

public class AuditServiceTests
{
    [Fact]
    public void Log_StagesEntryOnChangeTracker_WithoutSaving()
    {
        using var context = TestDbContextFactory.Create();
        var service = new AuditService(context);
        var entityId = Guid.NewGuid();

        service.Log(Guid.NewGuid(), AuditActions.OrderCreated, "Order", entityId, "details");

        Assert.Single(context.AuditLogs.Local);
        Assert.Empty(context.AuditLogs); // not yet persisted — caller hasn't called SaveChangesAsync
    }

    [Fact]
    public async Task Log_ThenSaveChanges_PersistsEntry()
    {
        using var context = TestDbContextFactory.Create();
        var service = new AuditService(context);
        var entityId = Guid.NewGuid();

        service.Log(Guid.NewGuid(), AuditActions.OrderStatusChanged, "Order", entityId, "Submitted -> Accepted");
        await context.SaveChangesAsync();

        var persisted = Assert.Single(context.AuditLogs);
        Assert.Equal(AuditActions.OrderStatusChanged, persisted.Action);
        Assert.Equal(entityId, persisted.EntityId);
    }
}
