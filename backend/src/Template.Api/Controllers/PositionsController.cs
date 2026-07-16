using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HireExam.Api.Localization;
using HireExam.Api.Middleware;
using HireExam.Application.Services;
using HireExam.Contracts.Common;
using HireExam.Contracts.Positions;

namespace HireExam.Api.Controllers;

[ApiController]
[Route("api/v1/positions")]
public sealed class PositionsController : ControllerBase
{
    private readonly IPositionService _positions;
    private readonly ILocalizer _loc;

    public PositionsController(IPositionService positions, ILocalizer loc)
    {
        _positions = positions;
        _loc = loc;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PositionResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
        => (await _positions.ListAsync(ct)).ToActionResult(_loc);

    [Authorize(Policy = "SuperAdminOnly")]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PositionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreatePositionRequest request, CancellationToken ct)
    {
        var userId = UserId();
        return (await _positions.CreateAsync(userId, request, ct)).ToActionResult(_loc);
    }

    [Authorize(Policy = "SuperAdminOnly")]
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
        => (await _positions.DeleteAsync(id, ct)).ToActionResult(_loc);

    private string UserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
}
