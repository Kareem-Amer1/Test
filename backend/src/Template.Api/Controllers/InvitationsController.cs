using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HireExam.Api.Extensions;
using HireExam.Api.Localization;
using HireExam.Api.Middleware;
using HireExam.Application.Common;
using HireExam.Application.Services;
using HireExam.Contracts.Common;
using HireExam.Contracts.Invitations;

namespace HireExam.Api.Controllers;

[ApiController]
[Route("api/v1/invitations")]
public sealed class InvitationsController : ControllerBase
{
    private readonly IInvitationService _invitations;
    private readonly ILocalizer _loc;

    public InvitationsController(IInvitationService invitations, ILocalizer loc)
    {
        _invitations = invitations;
        _loc = loc;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateInvitationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateInvitationRequest request, CancellationToken ct)
    {
        var result = await _invitations.CreateAsync(User.GetUserId(), request, ct);
        if (!result.IsSuccess || result.Value is null)
            return result.ToActionResult(_loc);

        var inviteUrl = $"{Request.Scheme}://{Request.Host}/invite/{result.Value.Token}";
        var enriched = result.Value with { InviteUrl = inviteUrl };
        return Result<CreateInvitationResponse>.Success(enriched).ToActionResult(_loc);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<InvitationListItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
        => (await _invitations.ListAsync(User.GetUserId(), User.GetRole(), ct)).ToActionResult(_loc);
}
