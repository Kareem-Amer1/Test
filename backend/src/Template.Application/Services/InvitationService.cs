using System.Security.Cryptography;
using HireExam.Application.Common;
using HireExam.Contracts.Invitations;
using HireExam.Core.Entities;
using HireExam.Core.Interfaces;

namespace HireExam.Application.Services;

public interface IInvitationService
{
    Task<Result<CreateInvitationResponse>> CreateAsync(string userId, CreateInvitationRequest request, CancellationToken ct = default);
    Task<Result<IReadOnlyList<InvitationListItemDto>>> ListAsync(string userId, string role, CancellationToken ct = default);
}

public sealed class InvitationService : IInvitationService
{
    private const int MinLinkHours = 1;
    private const int MaxLinkHours = 168;

    private readonly IExamInvitationRepository _invitations;
    private readonly IPositionRepository _positions;
    private readonly IUserRepository _users;

    public InvitationService(
        IExamInvitationRepository invitations,
        IPositionRepository positions,
        IUserRepository users)
    {
        _invitations = invitations;
        _positions = positions;
        _users = users;
    }

    public async Task<Result<CreateInvitationResponse>> CreateAsync(
        string userId, CreateInvitationRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result<CreateInvitationResponse>.Failure(ErrorCode.Unauthorized, "auth.unauthorized");

        if (request.LinkDurationHours < MinLinkHours || request.LinkDurationHours > MaxLinkHours)
            return Result<CreateInvitationResponse>.Failure(ErrorCode.Validation, "invitations.invalid_duration");

        var position = await _positions.GetByIdAsync(request.PositionId, ct);
        if (position is null)
            return Result<CreateInvitationResponse>.Failure(ErrorCode.NotFound, "positions.not_found");

        var user = await _users.GetByIdAsync(userId, ct);
        var now = DateTime.UtcNow;
        var invitation = new ExamInvitation
        {
            Token = GenerateToken(),
            PositionId = position.Id,
            PositionName = position.Name,
            CreatedBy = userId,
            CreatedByName = user?.FullName ?? user?.Email ?? "HR",
            LinkDurationHours = request.LinkDurationHours,
            LinkExpiresAt = now.AddHours(request.LinkDurationHours),
            Status = InvitationStatuses.Pending,
        };

        await _invitations.InsertAsync(invitation, ct);
        return Result<CreateInvitationResponse>.Success(new CreateInvitationResponse(
            invitation.Id,
            invitation.Token,
            invitation.PositionName,
            invitation.LinkExpiresAt,
            string.Empty));
    }

    public async Task<Result<IReadOnlyList<InvitationListItemDto>>> ListAsync(
        string userId, string role, CancellationToken ct = default)
    {
        var items = UserIdentity.IsSuperAdmin(role)
            ? await _invitations.ListAllAsync(ct)
            : await _invitations.ListByCreatorAsync(userId, ct);

        var now = DateTime.UtcNow;
        var mapped = items.Select(i => MapListItem(i, now)).ToList();
        return Result<IReadOnlyList<InvitationListItemDto>>.Success(mapped);
    }

    internal static InvitationListItemDto MapListItem(ExamInvitation invitation, DateTime now)
    {
        var status = ResolveDisplayStatus(invitation, now);
        return new InvitationListItemDto(
            invitation.Id,
            invitation.Token,
            invitation.PositionName,
            invitation.PositionId,
            invitation.LinkExpiresAt,
            invitation.LinkDurationHours,
            status,
            invitation.ExamId,
            invitation.CreatedAt);
    }

    internal static string ResolveDisplayStatus(ExamInvitation invitation, DateTime now)
    {
        if (invitation.Status == InvitationStatuses.Completed)
            return InvitationStatuses.Completed;
        if (invitation.Status == InvitationStatuses.InProgress)
            return InvitationStatuses.InProgress;
        if (invitation.LinkExpiresAt <= now)
            return InvitationStatuses.Expired;
        return invitation.Status;
    }

    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(24);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
