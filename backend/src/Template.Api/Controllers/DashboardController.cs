using Microsoft.AspNetCore.Mvc;
using HireExam.Api.Extensions;
using HireExam.Api.Localization;
using HireExam.Api.Middleware;
using HireExam.Application.Services;
using HireExam.Contracts.Common;
using HireExam.Contracts.Dashboard;
using HireExam.Core.Entities;

namespace HireExam.Api.Controllers;

[ApiController]
[Route("api/v1/dashboard")]
public sealed class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboard;
    private readonly ILocalizer _loc;

    public DashboardController(IDashboardService dashboard, ILocalizer loc)
    {
        _dashboard = dashboard;
        _loc = loc;
    }

    [HttpGet("stats")]
    [ProducesResponseType(typeof(ApiResponse<DashboardStatsResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Stats(CancellationToken ct)
        => (await _dashboard.GetStatsAsync(User.GetUserId(), User.GetRole(), ct)).ToActionResult(_loc);
}
