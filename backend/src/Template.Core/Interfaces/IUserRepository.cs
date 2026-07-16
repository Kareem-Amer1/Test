using VoiceFlowStudio.Core.Entities;

namespace VoiceFlowStudio.Core.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByIdAsync(string id, CancellationToken ct = default);
    Task InsertAsync(User user, CancellationToken ct = default);

    /// <summary>Append a project ID to the user's ownership list (single-document write).</summary>
    Task<bool> AddProjectIdAsync(string userId, string projectId, CancellationToken ct = default);

    /// <summary>Remove a project ID from the user's ownership list (single-document write).</summary>
    Task<bool> RemoveProjectIdAsync(string userId, string projectId, CancellationToken ct = default);
}
