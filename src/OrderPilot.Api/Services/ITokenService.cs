using OrderPilot.Api.Domain.Entities;

namespace OrderPilot.Api.Services;

public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateToken(ApplicationUser user, string role);
}
