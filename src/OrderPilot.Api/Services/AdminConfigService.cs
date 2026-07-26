using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrderPilot.Api.Data;
using OrderPilot.Api.Domain.Entities;

namespace OrderPilot.Api.Services;

public class AdminConfigService : IAdminConfigService
{
    public const string CustomerRole = "Customer";

    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminConfigService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<List<ApplicationUser>> GetCustomersAsync()
    {
        var customerIds = await _userManager.GetUsersInRoleAsync(CustomerRole);
        return customerIds.OrderByDescending(u => u.CreatedAtUtc).ToList();
    }

    public async Task<ApplicationUser> CreateCustomerAsync(string email, string password, string? companyName, bool isPilotActive)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            CompanyName = companyName,
            IsPilotActive = isPilotActive,
            CreatedAtUtc = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create customer: {errors}");
        }

        await _userManager.AddToRoleAsync(user, CustomerRole);

        return user;
    }

    public async Task<ApplicationUser> UpdateCustomerAsync(Guid customerId, bool isPilotActive, string? companyName)
    {
        var user = await _userManager.FindByIdAsync(customerId.ToString())
            ?? throw new KeyNotFoundException($"Customer '{customerId}' was not found.");

        user.IsPilotActive = isPilotActive;
        if (companyName is not null)
        {
            user.CompanyName = companyName;
        }

        await _userManager.UpdateAsync(user);
        return user;
    }

    public async Task<List<Supplier>> GetSuppliersAsync()
    {
        return await _context.Suppliers.OrderByDescending(s => s.CreatedAtUtc).ToListAsync();
    }

    public async Task<Supplier> CreateSupplierAsync(string name, string code, bool isActive)
    {
        if (await _context.Suppliers.AnyAsync(s => s.Code == code))
        {
            throw new DuplicateSupplierCodeException();
        }

        if (isActive)
        {
            // Single-active-supplier invariant: staged on the change tracker here and
            // committed together with the new supplier in the single SaveChangesAsync
            // below, so both changes land atomically in one unit of work.
            await DeactivateAllSuppliersAsync();
        }

        var supplier = new Supplier
        {
            Id = Guid.NewGuid(),
            Name = name,
            Code = code,
            IsActive = isActive,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        return supplier;
    }

    public async Task<Supplier> UpdateSupplierAsync(Guid supplierId, bool isActive, string? name)
    {
        var supplier = await _context.Suppliers.SingleOrDefaultAsync(s => s.Id == supplierId)
            ?? throw new KeyNotFoundException($"Supplier '{supplierId}' was not found.");

        if (isActive && !supplier.IsActive)
        {
            // Single-active-supplier invariant: activating this one deactivates every other,
            // staged alongside this supplier's own update in the same SaveChangesAsync call
            // below so both commit atomically.
            await DeactivateAllSuppliersAsync();
        }

        supplier.IsActive = isActive;
        if (name is not null)
        {
            supplier.Name = name;
        }
        supplier.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return supplier;
    }

    public async Task<List<AuditLog>> GetAuditLogsAsync()
    {
        return await _context.AuditLogs.OrderByDescending(a => a.TimestampUtc).ToListAsync();
    }

    private async Task DeactivateAllSuppliersAsync()
    {
        // Loads and mutates via the change tracker (rather than ExecuteUpdateAsync) so this
        // stages alongside the caller's own SaveChangesAsync in the same unit of work, and
        // works identically across every EF Core provider used in this codebase (SQL Server,
        // SQLite in integration tests, InMemory in unit tests).
        var activeSuppliers = await _context.Suppliers.Where(s => s.IsActive).ToListAsync();
        foreach (var supplier in activeSuppliers)
        {
            supplier.IsActive = false;
            supplier.UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}
