using VoiceFlowStudio.Core.Entities;

namespace VoiceFlowStudio.Core.Interfaces;

/// <summary>
/// Project documents no longer hold a back-reference to a user; ownership
/// is resolved via the user's <c>ProjectIds</c> list. Per Constitution IX,
/// queries support optional field projection.
/// </summary>
public interface IProjectRepository
{
    Task<IReadOnlyList<Project>> ListByIdsAsync(IReadOnlyCollection<string> ids, IReadOnlyCollection<string>? fields = null, CancellationToken ct = default);
    Task<Project?> GetByIdAsync(string id, CancellationToken ct = default);
    Task InsertAsync(Project project, CancellationToken ct = default);
    Task<bool> UpdateAsync(Project project, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
}
