namespace HireExam.Contracts.Invitations;

public sealed record CreateInvitationRequest(string PositionId, int LinkDurationHours);

public sealed record CreateInvitationResponse(
    string Id,
    string Token,
    string PositionName,
    DateTime LinkExpiresAt,
    string InviteUrl);

public sealed record InvitationListItemDto(
    string Id,
    string Token,
    string PositionName,
    string PositionId,
    DateTime LinkExpiresAt,
    int LinkDurationHours,
    string Status,
    string? ExamId,
    DateTime CreatedAt);

public sealed record InviteInfoResponse(
    string PositionName,
    string Status,
    bool CanStart,
    bool CanResume,
    bool IsCompleted,
    bool IsExpired,
    DateTime LinkExpiresAt);

public sealed record StartCandidateExamRequest(
    string FullName,
    string Email,
    string Mobile);

public sealed record StartCandidateExamResponse(
    string ExamId,
    string CandidateName,
    string PositionName,
    string Status);
