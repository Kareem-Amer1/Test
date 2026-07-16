using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VoiceFlowStudio.Api.Localization;
using VoiceFlowStudio.Api.Middleware;
using VoiceFlowStudio.Application.Services;
using VoiceFlowStudio.Contracts.Common;
using VoiceFlowStudio.Contracts.Projects;

namespace VoiceFlowStudio.Api.Controllers;

[ApiController]
[Route("api/v1/projects")]
public sealed class ProjectsController : ControllerBase
{
    private readonly IProjectService _projects;
    private readonly ILocalizer _loc;

    public ProjectsController(IProjectService projects, ILocalizer loc)
    {
        _projects = projects;
        _loc = loc;
    }

    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;

    /// <summary>List the caller's projects.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ProjectResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
        => (await _projects.ListAsync(CurrentUserId, ct)).ToActionResult(_loc);

    /// <summary>Get a single project by ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ProjectResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProjectResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(string id, CancellationToken ct)
        => (await _projects.GetAsync(id, CurrentUserId, ct)).ToActionResult(_loc);

    /// <summary>Create a project owned by the caller.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProjectResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ProjectResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequest request, CancellationToken ct)
        => (await _projects.CreateAsync(CurrentUserId, request, ct)).ToActionResult(_loc, StatusCodes.Status201Created);

    /// <summary>Patch project fields.</summary>
    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ProjectResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProjectResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateProjectRequest request, CancellationToken ct)
        => (await _projects.UpdateAsync(id, CurrentUserId, request, ct)).ToActionResult(_loc);

    /// <summary>Delete a project.</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
        => (await _projects.DeleteAsync(id, CurrentUserId, ct)).ToActionResult(_loc);
}
