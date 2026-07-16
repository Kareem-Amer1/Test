using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HireExam.Api.Localization;
using HireExam.Api.Middleware;
using HireExam.Application.Services;
using HireExam.Contracts.Common;
using HireExam.Contracts.Exams;
using HireExam.Core.Entities;

namespace HireExam.Api.Controllers;

[ApiController]
[Route("api/v1/exams")]
public sealed class ExamsController : ControllerBase
{
    private readonly IExamService _exams;
    private readonly ILocalizer _loc;

    public ExamsController(IExamService exams, ILocalizer loc)
    {
        _exams = exams;
        _loc = loc;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ExamListItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] string? positionId,
        [FromQuery] string? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct)
    {
        var query = new ExamListQueryDto(positionId, status, from, to);
        return (await _exams.ListAsync(UserId(), Role(), query, ct)).ToActionResult(_loc);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateExamResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateExamRequest request, CancellationToken ct)
        => (await _exams.CreateAsync(UserId(), request, ct)).ToActionResult(_loc);

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ExamDetailResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDetail(string id, CancellationToken ct)
        => (await _exams.GetDetailAsync(id, UserId(), Role(), ct)).ToActionResult(_loc);

    [HttpGet("{id}/session")]
    [ProducesResponseType(typeof(ApiResponse<ExamSessionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSession(string id, CancellationToken ct)
        => (await _exams.GetSessionAsync(id, UserId(), Role(), ct)).ToActionResult(_loc);

    [HttpPut("{id}/answers")]
    [ProducesResponseType(typeof(ApiResponse<ExamSessionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SaveAnswers(string id, [FromBody] SaveAnswersRequest request, CancellationToken ct)
        => (await _exams.SaveAnswersAsync(id, UserId(), Role(), request, ct)).ToActionResult(_loc);

    [HttpPost("{id}/submit")]
    [ProducesResponseType(typeof(ApiResponse<SubmitExamResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Submit(string id, [FromBody] SubmitExamRequest? request, CancellationToken ct)
        => (await _exams.SubmitAsync(id, UserId(), Role(), request, ct)).ToActionResult(_loc);

    [HttpPut("{id}/grade")]
    [ProducesResponseType(typeof(ApiResponse<GradeExamResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Grade(string id, [FromBody] GradeExamRequest request, CancellationToken ct)
        => (await _exams.GradeAsync(id, UserId(), Role(), request, ct)).ToActionResult(_loc);

    private string UserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;

    private string Role() => User.FindFirstValue("role") ?? UserRoles.HR;
}
