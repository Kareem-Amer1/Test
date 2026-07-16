using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VoiceFlowStudio.Api.Localization;
using VoiceFlowStudio.Api.Middleware;
using VoiceFlowStudio.Application.Services;
using VoiceFlowStudio.Contracts.Auth;
using VoiceFlowStudio.Contracts.Common;

namespace VoiceFlowStudio.Api.Controllers;

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

    /// <summary>Register a new user. Anonymous signups must be enabled at the application layer.</summary>
    [AllowAnonymous] // Justification: public registration endpoint (Constitution IV exclusion: /api/v1/auth/*)
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthTokensResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthTokensResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<AuthTokensResponse>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
        => (await _auth.RegisterAsync(request, ct)).ToActionResult(_loc);

    /// <summary>Exchange email + password for a JWT access token and refresh token.</summary>
    [AllowAnonymous] // Justification: login endpoint
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthTokensResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthTokensResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
        => (await _auth.LoginAsync(request, ct)).ToActionResult(_loc);

    /// <summary>Rotate the access token using a valid refresh token.</summary>
    [AllowAnonymous] // Justification: refresh endpoint, validated against stored token hash
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<AuthTokensResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthTokensResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken ct)
        => (await _auth.RefreshAsync(request, ct)).ToActionResult(_loc);

    /// <summary>Revoke all active refresh tokens for the caller.</summary>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        return (await _auth.LogoutAsync(userId, ct)).ToActionResult(_loc);
    }
}
