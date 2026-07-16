using MongoDB.Driver;
using VoiceFlowStudio.Core.Entities;
using VoiceFlowStudio.Core.Interfaces;
using VoiceFlowStudio.Infrastructure.Persistence;

namespace VoiceFlowStudio.Infrastructure.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IMongoCollection<RefreshToken> _col;

    public RefreshTokenRepository(IMongoClientFactory factory)
    {
        _col = factory.GetDatabase().GetCollection<RefreshToken>("refresh_tokens");
    }

    public Task InsertAsync(RefreshToken token, CancellationToken ct = default) =>
        _col.InsertOneAsync(token, cancellationToken: ct);

    public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct = default) =>
        _col.Find(r => r.TokenHash == tokenHash).FirstOrDefaultAsync(ct)!;

    public Task RevokeAsync(string id, CancellationToken ct = default) =>
        _col.UpdateOneAsync(r => r.Id == id,
            Builders<RefreshToken>.Update.Set(r => r.RevokedAt, DateTime.UtcNow), cancellationToken: ct);

    public Task RevokeAllForUserAsync(string userId, CancellationToken ct = default) =>
        _col.UpdateManyAsync(r => r.UserId == userId && r.RevokedAt == null,
            Builders<RefreshToken>.Update.Set(r => r.RevokedAt, DateTime.UtcNow), cancellationToken: ct);
}
