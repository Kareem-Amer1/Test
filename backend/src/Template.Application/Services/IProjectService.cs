using VoiceFlowStudio.Application.Common;
using VoiceFlowStudio.Contracts.Projects;

namespace VoiceFlowStudio.Application.Services;

public interface IProjectService
{
    Task<Result<IReadOnlyList<ProjectResponse>>> ListAsync(string userId, CancellationToken ct = default);
    Task<Result<ProjectResponse>> GetAsync(string id, string userId, CancellationToken ct = default);
    Task<Result<ProjectResponse>> CreateAsync(string userId, CreateProjectRequest request, CancellationToken ct = default);
    Task<Result<ProjectResponse>> UpdateAsync(string id, string userId, UpdateProjectRequest request, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(string id, string userId, CancellationToken ct = default);
}
