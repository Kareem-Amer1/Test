using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HireExam.Api.Localization;
using HireExam.Api.Middleware;
using HireExam.Application.Services;
using HireExam.Contracts.Auth;
using HireExam.Contracts.Common;
using HireExam.Infrastructure.Seed;

namespace HireExam.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly ILocalizer _loc;

    public AuthController(IAuthService auth, ILocalizer loc)
    {
        _auth = auth;
        _loc = loc;
    }

    /// <summary>Exchange email + password for a JWT access token and refresh token.</summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthTokensResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthTokensResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
        => (await _auth.LoginAsync(request, ct)).ToActionResult(_loc);

    /// <summary>Rotate the access token using a valid refresh token.</summary>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<AuthTokensResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthTokensResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken ct)
        => (await _auth.RefreshAsync(request, ct)).ToActionResult(_loc);

    /// <summary>Get the authenticated user's profile.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        return (await _auth.GetMeAsync(userId, ct)).ToActionResult(_loc);
    }

    /// <summary>Revoke all active refresh tokens for the caller.</summary>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        return (await _auth.LogoutAsync(userId, ct)).ToActionResult(_loc);
    }
}

[ApiController]
[Route("api/v1/seed")]
public sealed class SeedController : ControllerBase
{
    private readonly ISeedService _seed;

    public SeedController(ISeedService seed) => _seed = seed;

    /// <summary>Idempotently seed the Super Admin account if none exists.</summary>
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Seed(CancellationToken ct)
    {
        await _seed.SeedSuperAdminIfNeededAsync(ct);
        return Ok(ApiResponse<bool>.Ok(true));
    }
}
