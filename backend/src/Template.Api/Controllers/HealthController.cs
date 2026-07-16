using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VoiceFlowStudio.Api.Controllers;

[ApiController]
public sealed class HealthController : ControllerBase
{
    /// <summary>Liveness probe (Constitution IV exclusion).</summary>
    [AllowAnonymous]
    [HttpGet("/health")]
    public IActionResult Health() => Ok(new { status = "ok" });

    /// <summary>Readiness probe (Constitution IV exclusion).</summary>
    [AllowAnonymous]
    [HttpGet("/ready")]
    public IActionResult Ready() => Ok(new { status = "ready" });
}
