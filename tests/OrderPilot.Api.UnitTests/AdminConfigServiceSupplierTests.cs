using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OrderPilot.Api.Domain.Entities;
using OrderPilot.Api.Services;
using Xunit;

namespace OrderPilot.Api.UnitTests;

public class AdminConfigServiceSupplierTests
{
    // UserManager<T> has no parameterless/mockable-friendly constructor path without a
    // full DI-backed IUserStore, so these tests exercise only the supplier-management
    // surface (pure DbContext operations). Customer-management flows that go through
    // UserManager are covered by the integration test suite instead.
    private static AdminConfigService CreateService(Data.ApplicationDbContext context)
    {
        var storeMock = new Mock<IUserStore<ApplicationUser>>();
        var userManager = new UserManager<ApplicationUser>(
            storeMock.Object, null!, null!, [], [], null!, null!, null!,
            NullLogger<UserManager<ApplicationUser>>.Instance);

        return new AdminConfigService(context, userManager);
    }

    [Fact]
    public async Task CreateSupplierAsync_DuplicateCode_ThrowsDuplicateSupplierCodeException()
    {
        using var context = TestDbContextFactory.Create();
        context.Suppliers.Add(new Supplier { Id = Guid.NewGuid(), Name = "Acme", Code = "ACME", IsActive = false });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        await Assert.ThrowsAsync<DuplicateSupplierCodeException>(
            () => service.CreateSupplierAsync("Acme Foods Inc", "ACME", isActive: false));
    }

    [Fact]
    public async Task CreateSupplierAsync_ActivatingNewSupplier_DeactivatesExistingActiveSupplier()
    {
        using var context = TestDbContextFactory.Create();
        var existing = new Supplier { Id = Guid.NewGuid(), Name = "Old Supplier", Code = "OLD", IsActive = true };
        context.Suppliers.Add(existing);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var created = await service.CreateSupplierAsync("New Supplier", "NEW", isActive: true);

        var reloadedExisting = await context.Suppliers.FindAsync(existing.Id);
        Assert.False(reloadedExisting!.IsActive);
        Assert.True(created.IsActive);
    }

    [Fact]
    public async Task UpdateSupplierAsync_ActivatingSupplier_DeactivatesOtherActiveSuppliers()
    {
        using var context = TestDbContextFactory.Create();
        var supplierA = new Supplier { Id = Guid.NewGuid(), Name = "A", Code = "A", IsActive = true };
        var supplierB = new Supplier { Id = Guid.NewGuid(), Name = "B", Code = "B", IsActive = false };
        context.Suppliers.AddRange(supplierA, supplierB);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        await service.UpdateSupplierAsync(supplierB.Id, isActive: true, name: null);

        var reloadedA = await context.Suppliers.FindAsync(supplierA.Id);
        var reloadedB = await context.Suppliers.FindAsync(supplierB.Id);
        Assert.False(reloadedA!.IsActive);
        Assert.True(reloadedB!.IsActive);
    }

    [Fact]
    public async Task UpdateSupplierAsync_UnknownSupplier_ThrowsKeyNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var service = CreateService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.UpdateSupplierAsync(Guid.NewGuid(), isActive: true, name: null));
    }
}
