using VoiceFlowStudio.Core.Entities;

namespace VoiceFlowStudio.Core.Interfaces;

public interface IRefreshTokenRepository
{
    Task InsertAsync(RefreshToken token, CancellationToken ct = default);
    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct = default);
    Task RevokeAsync(string id, CancellationToken ct = default);
    Task RevokeAllForUserAsync(string userId, CancellationToken ct = default);
}
