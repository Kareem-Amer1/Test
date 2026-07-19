using Microsoft.AspNetCore.Mvc;
using HireExam.Api.Extensions;
using HireExam.Api.Localization;
using HireExam.Api.Middleware;
using HireExam.Application.Services;
using HireExam.Contracts.Common;
using HireExam.Contracts.Exams;

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
        [FromQuery] string? search,
        CancellationToken ct)
    {
        var query = new ExamListQueryDto(positionId, status, from, to, search);
        return (await _exams.ListAsync(User.GetUserId(), User.GetRole(), query, ct)).ToActionResult(_loc);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ExamDetailResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDetail(string id, CancellationToken ct)
        => (await _exams.GetDetailAsync(id, User.GetUserId(), User.GetRole(), ct)).ToActionResult(_loc);

    [HttpGet("{id}/session")]
    [ProducesResponseType(typeof(ApiResponse<ExamSessionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSession(string id, CancellationToken ct)
        => (await _exams.GetSessionAsync(id, User.GetUserId(), User.GetRole(), ct)).ToActionResult(_loc);

    [HttpPut("{id}/answers")]
    [ProducesResponseType(typeof(ApiResponse<ExamSessionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SaveAnswers(string id, [FromBody] SaveAnswersRequest request, CancellationToken ct)
        => (await _exams.SaveAnswersAsync(id, User.GetUserId(), User.GetRole(), request, ct)).ToActionResult(_loc);

    [HttpPost("{id}/submit")]
    [ProducesResponseType(typeof(ApiResponse<SubmitExamResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Submit(string id, [FromBody] SubmitExamRequest? request, CancellationToken ct)
        => (await _exams.SubmitAsync(id, User.GetUserId(), User.GetRole(), request, ct)).ToActionResult(_loc);

    [HttpPut("{id}/grade")]
    [ProducesResponseType(typeof(ApiResponse<GradeExamResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Grade(string id, [FromBody] GradeExamRequest request, CancellationToken ct)
        => (await _exams.GradeAsync(id, User.GetUserId(), User.GetRole(), request, ct)).ToActionResult(_loc);
}
