using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OrderPilot.Api.Domain.Entities;
using OrderPilot.Api.Dtos.Auth;
using OrderPilot.Api.Services;

namespace OrderPilot.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;

    public AuthController(UserManager<ApplicationUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault();
        if (role is null)
        {
            return Unauthorized(new { message = "User has no assigned role." });
        }

        if (role == AdminConfigService.CustomerRole && !user.IsPilotActive)
        {
            return Unauthorized(new { message = "This account is not active in the pilot." });
        }

        var (token, expiresAtUtc) = _tokenService.CreateToken(user, role);

        return Ok(new LoginResponse
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            Role = role
        });
    }
}
