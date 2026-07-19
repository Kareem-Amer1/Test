using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HireExam.Api.Extensions;
using HireExam.Api.Localization;
using HireExam.Api.Middleware;
using HireExam.Application.Services;
using HireExam.Contracts.Common;
using HireExam.Contracts.Exams;
using HireExam.Contracts.Invitations;

namespace HireExam.Api.Controllers;

[ApiController]
[Route("api/v1/invite")]
[AllowAnonymous]
public sealed class InviteController : ControllerBase
{
    private readonly ICandidateExamService _candidateExams;
    private readonly ILocalizer _loc;

    public InviteController(ICandidateExamService candidateExams, ILocalizer loc)
    {
        _candidateExams = candidateExams;
        _loc = loc;
    }

    [HttpGet("{token}")]
    [ProducesResponseType(typeof(ApiResponse<InviteInfoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInfo(string token, CancellationToken ct)
        => (await _candidateExams.GetInviteInfoAsync(token, ct)).ToActionResult(_loc);

    [HttpPost("{token}/start")]
    [ProducesResponseType(typeof(ApiResponse<ExamSessionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Start(string token, [FromBody] StartCandidateExamRequest request, CancellationToken ct)
        => (await _candidateExams.StartAsync(token, request, ct)).ToActionResult(_loc);

    [HttpGet("{token}/session")]
    [ProducesResponseType(typeof(ApiResponse<ExamSessionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSession(string token, CancellationToken ct)
        => (await _candidateExams.GetSessionAsync(token, ct)).ToActionResult(_loc);

    [HttpPut("{token}/answers")]
    [ProducesResponseType(typeof(ApiResponse<ExamSessionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SaveAnswers(string token, [FromBody] SaveAnswersRequest request, CancellationToken ct)
        => (await _candidateExams.SaveAnswersAsync(token, request, ct)).ToActionResult(_loc);

    [HttpPost("{token}/submit")]
    [ProducesResponseType(typeof(ApiResponse<SubmitExamResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Submit(string token, [FromBody] SubmitExamRequest? request, CancellationToken ct)
        => (await _candidateExams.SubmitAsync(token, request, ct)).ToActionResult(_loc);
}
