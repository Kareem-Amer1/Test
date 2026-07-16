namespace VoiceFlowStudio.Contracts.Projects;

public sealed record ProjectResponse(
    string Id,
    string Name,
    string Description,
    string Color,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record CreateProjectRequest(string Name, string Description, string Color);
public sealed record UpdateProjectRequest(string? Name, string? Description, string? Color);
