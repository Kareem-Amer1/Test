using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HireExam.Api.Localization;
using HireExam.Api.Middleware;
using HireExam.Application.Services;
using HireExam.Contracts.Common;
using HireExam.Contracts.Templates;

namespace HireExam.Api.Controllers;

[ApiController]
[Route("api/v1/positions/{positionId}/template")]
public sealed class TemplatesController : ControllerBase
{
    private readonly ITemplateService _templates;
    private readonly ILocalizer _loc;

    public TemplatesController(ITemplateService templates, ILocalizer loc)
    {
        _templates = templates;
        _loc = loc;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<TemplateResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(string positionId, CancellationToken ct)
        => (await _templates.GetByPositionIdAsync(positionId, ct)).ToActionResult(_loc);

    [HttpPut("duration")]
    [ProducesResponseType(typeof(ApiResponse<TemplateResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateDuration(string positionId, [FromBody] UpdateDurationRequest request, CancellationToken ct)
        => (await _templates.UpdateDurationAsync(positionId, UserId(), request, ct)).ToActionResult(_loc);

    [HttpPost("partitions")]
    [ProducesResponseType(typeof(ApiResponse<TemplateResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddPartition(string positionId, [FromBody] UpsertPartitionRequest request, CancellationToken ct)
        => (await _templates.AddPartitionAsync(positionId, UserId(), request, ct)).ToActionResult(_loc);

    [HttpPut("partitions/{partitionId}")]
    [ProducesResponseType(typeof(ApiResponse<TemplateResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdatePartition(string positionId, string partitionId, [FromBody] UpsertPartitionRequest request, CancellationToken ct)
        => (await _templates.UpdatePartitionAsync(positionId, UserId(), partitionId, request, ct)).ToActionResult(_loc);

    [HttpDelete("partitions/{partitionId}")]
    [ProducesResponseType(typeof(ApiResponse<TemplateResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeletePartition(string positionId, string partitionId, CancellationToken ct)
        => (await _templates.DeletePartitionAsync(positionId, UserId(), partitionId, ct)).ToActionResult(_loc);

    [HttpPost("partitions/{partitionId}/questions")]
    [ProducesResponseType(typeof(ApiResponse<TemplateResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddQuestion(string positionId, string partitionId, [FromBody] UpsertQuestionRequest request, CancellationToken ct)
        => (await _templates.AddQuestionAsync(positionId, UserId(), partitionId, request, ct)).ToActionResult(_loc);

    [HttpPut("partitions/{partitionId}/questions/{questionId}")]
    [ProducesResponseType(typeof(ApiResponse<TemplateResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateQuestion(string positionId, string partitionId, string questionId, [FromBody] UpsertQuestionRequest request, CancellationToken ct)
        => (await _templates.UpdateQuestionAsync(positionId, UserId(), partitionId, questionId, request, ct)).ToActionResult(_loc);

    [HttpDelete("partitions/{partitionId}/questions/{questionId}")]
    [ProducesResponseType(typeof(ApiResponse<TemplateResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteQuestion(string positionId, string partitionId, string questionId, CancellationToken ct)
        => (await _templates.DeleteQuestionAsync(positionId, UserId(), partitionId, questionId, ct)).ToActionResult(_loc);

    [HttpPut("partitions/{partitionId}/questions/reorder")]
    [ProducesResponseType(typeof(ApiResponse<TemplateResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Reorder(string positionId, string partitionId, [FromBody] ReorderQuestionsRequest request, CancellationToken ct)
        => (await _templates.ReorderQuestionsAsync(positionId, UserId(), partitionId, request, ct)).ToActionResult(_loc);

    private string UserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
}
