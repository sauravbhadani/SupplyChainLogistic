using OrderPilot.Api.Domain.Entities;

namespace OrderPilot.Api.Services;

public class DuplicateSupplierCodeException : Exception
{
}

public interface IAdminConfigService
{
    Task<List<ApplicationUser>> GetCustomersAsync();

    Task<ApplicationUser> CreateCustomerAsync(string email, string password, string? companyName, bool isPilotActive);

    Task<ApplicationUser> UpdateCustomerAsync(Guid customerId, bool isPilotActive, string? companyName);

    Task<List<Supplier>> GetSuppliersAsync();

    Task<Supplier> CreateSupplierAsync(string name, string code, bool isActive);

    Task<Supplier> UpdateSupplierAsync(Guid supplierId, bool isActive, string? name);

    Task<List<AuditLog>> GetAuditLogsAsync();
}
