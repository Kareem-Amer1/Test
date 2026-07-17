using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HireExam.Api.Localization;
using HireExam.Api.Middleware;
using HireExam.Application.Services;
using HireExam.Contracts.Common;
using HireExam.Contracts.Users;

namespace HireExam.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize(Policy = "SuperAdminOnly")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _users;
    private readonly ILocalizer _loc;

    public UsersController(IUserService users, ILocalizer loc)
    {
        _users = users;
        _loc = loc;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<HrUserResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
        => (await _users.ListHrUsersAsync(ct)).ToActionResult(_loc);

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<HrUserResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateHrUserRequest request, CancellationToken ct)
        => (await _users.CreateHrUserAsync(UserId(), request, ct)).ToActionResult(_loc);

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Deactivate(string id, CancellationToken ct)
        => (await _users.DeactivateHrUserAsync(id, UserId(), ct)).ToActionResult(_loc);

    private string UserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
}
