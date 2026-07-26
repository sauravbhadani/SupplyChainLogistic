using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrderPilot.Api.Data;
using OrderPilot.Api.Domain.Entities;
using OrderPilot.Api.Dtos.Auth;
using OrderPilot.Api.Services;

namespace OrderPilot.Api.IntegrationTests;

public static class TestDataHelper
{
    public const string DefaultPassword = "TestPass#2026";

    public static async Task EnsureRolesAsync(CustomWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        foreach (var role in new[] { "Admin", AdminConfigService.CustomerRole })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }
    }

    public static async Task<ApplicationUser> CreateAdminAsync(CustomWebApplicationFactory factory, string email)
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            IsPilotActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        await userManager.CreateAsync(user, DefaultPassword);
        await userManager.AddToRoleAsync(user, "Admin");
        return user;
    }

    public static async Task<ApplicationUser> CreateCustomerAsync(
        CustomWebApplicationFactory factory, string email, bool isPilotActive = true)
    {
        using var scope = factory.Services.CreateScope();
        var adminConfigService = scope.ServiceProvider.GetRequiredService<IAdminConfigService>();
        return await adminConfigService.CreateCustomerAsync(email, DefaultPassword, companyName: null, isPilotActive);
    }

    public static async Task<Supplier> CreateActiveSupplierAsync(CustomWebApplicationFactory factory, string code)
    {
        using var scope = factory.Services.CreateScope();
        var adminConfigService = scope.ServiceProvider.GetRequiredService<IAdminConfigService>();
        return await adminConfigService.CreateSupplierAsync($"Supplier {code}", code, isActive: true);
    }

    public static async Task<Order> CreateOrderDirectlyAsync(
        CustomWebApplicationFactory factory, Guid customerId, string productDescription = "Widgets")
    {
        using var scope = factory.Services.CreateScope();
        var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
        return await orderService.CreateOrderAsync(customerId, productDescription, quantity: 1, notes: null);
    }

    public static async Task<string> LoginAsync(HttpClient client, string email, string password = DefaultPassword)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest { Email = email, Password = password });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return body!.Token;
    }

    public static HttpClient WithBearerToken(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
