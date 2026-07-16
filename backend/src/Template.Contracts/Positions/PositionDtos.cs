namespace HireExam.Contracts.Positions;

public sealed record PositionResponse(
    string Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt);

public sealed record CreatePositionRequest(string Name, string? Description);
