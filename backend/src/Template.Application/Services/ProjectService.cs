using VoiceFlowStudio.Application.Common;
using VoiceFlowStudio.Contracts.Projects;
using VoiceFlowStudio.Core.Entities;
using VoiceFlowStudio.Core.Interfaces;

namespace VoiceFlowStudio.Application.Services;

/// <summary>
/// Application service for project CRUD. Ownership is resolved through
/// <see cref="User.ProjectIds"/> — the project document itself carries no
/// reference to the user. Each mutation is a single-document write per
/// Constitution VIII (no transactions).
/// </summary>
public sealed class ProjectService : IProjectService
{
    private readonly IProjectRepository _projects;
    private readonly IUserRepository _users;

    public ProjectService(IProjectRepository projects, IUserRepository users)
    {
        _projects = projects;
        _users = users;
    }

    public async Task<Result<IReadOnlyList<ProjectResponse>>> ListAsync(string userId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result<IReadOnlyList<ProjectResponse>>.Failure(ErrorCode.Unauthorized, "auth.unauthorized");

        var user = await _users.GetByIdAsync(userId, ct);
        if (user is null)
            return Result<IReadOnlyList<ProjectResponse>>.Failure(ErrorCode.Unauthorized, "auth.unauthorized");
        if (user.ProjectIds.Count == 0)
            return Result<IReadOnlyList<ProjectResponse>>.Success(Array.Empty<ProjectResponse>());

        var list = await _projects.ListByIdsAsync(user.ProjectIds, fields: null, ct);
        return Result<IReadOnlyList<ProjectResponse>>.Success(list.Select(ToDto).ToList());
    }

    public async Task<Result<ProjectResponse>> GetAsync(string id, string userId, CancellationToken ct = default)
    {
        if (!await UserOwnsAsync(userId, id, ct))
            return Result<ProjectResponse>.Failure(ErrorCode.NotFound, "projects.not_found");

        var project = await _projects.GetByIdAsync(id, ct);
        if (project is null)
            return Result<ProjectResponse>.Failure(ErrorCode.NotFound, "projects.not_found");
        return Result<ProjectResponse>.Success(ToDto(project));
    }

    public async Task<Result<ProjectResponse>> CreateAsync(string userId, CreateProjectRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result<ProjectResponse>.Failure(ErrorCode.Unauthorized, "auth.unauthorized");
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result<ProjectResponse>.Failure(ErrorCode.Validation, "projects.name_required");

        var entity = new Project
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Color = string.IsNullOrWhiteSpace(request.Color) ? "#3b82f6" : request.Color,
        };
        await _projects.InsertAsync(entity, ct);
        await _users.AddProjectIdAsync(userId, entity.Id, ct);
        return Result<ProjectResponse>.Success(ToDto(entity));
    }

    public async Task<Result<ProjectResponse>> UpdateAsync(string id, string userId, UpdateProjectRequest request, CancellationToken ct = default)
    {
        if (!await UserOwnsAsync(userId, id, ct))
            return Result<ProjectResponse>.Failure(ErrorCode.NotFound, "projects.not_found");

        var project = await _projects.GetByIdAsync(id, ct);
        if (project is null)
            return Result<ProjectResponse>.Failure(ErrorCode.NotFound, "projects.not_found");

        if (request.Name is not null) project.Name = request.Name.Trim();
        if (request.Description is not null) project.Description = request.Description.Trim();
        if (request.Color is not null) project.Color = request.Color;
        project.UpdatedAt = DateTime.UtcNow;

        var ok = await _projects.UpdateAsync(project, ct);
        return ok
            ? Result<ProjectResponse>.Success(ToDto(project))
            : Result<ProjectResponse>.Failure(ErrorCode.Unexpected, "projects.update_failed");
    }

    public async Task<Result<bool>> DeleteAsync(string id, string userId, CancellationToken ct = default)
    {
        if (!await UserOwnsAsync(userId, id, ct))
            return Result<bool>.Failure(ErrorCode.NotFound, "projects.not_found");

        var ok = await _projects.DeleteAsync(id, ct);
        if (!ok)
            return Result<bool>.Failure(ErrorCode.NotFound, "projects.not_found");

        await _users.RemoveProjectIdAsync(userId, id, ct);
        return Result<bool>.Success(true);
    }

    private async Task<bool> UserOwnsAsync(string userId, string projectId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userId)) return false;
        var user = await _users.GetByIdAsync(userId, ct);
        return user is not null && user.ProjectIds.Contains(projectId);
    }

    private static ProjectResponse ToDto(Project p) =>
        new(p.Id, p.Name, p.Description, p.Color, p.CreatedAt, p.UpdatedAt);
}
