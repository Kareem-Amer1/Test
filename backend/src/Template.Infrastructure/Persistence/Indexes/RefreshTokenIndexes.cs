using MongoDB.Driver;
using VoiceFlowStudio.Core.Entities;

namespace VoiceFlowStudio.Infrastructure.Persistence.Indexes;

public static class RefreshTokenIndexes
{
    public static Task EnsureAsync(IMongoDatabase db, CancellationToken ct = default)
    {
        var col = db.GetCollection<RefreshToken>("refresh_tokens");
        var hashUnique = new CreateIndexModel<RefreshToken>(
            Builders<RefreshToken>.IndexKeys.Ascending(r => r.TokenHash),
            new CreateIndexOptions { Unique = true, Name = "ux_refresh_tokenHash" });
        var ttl = new CreateIndexModel<RefreshToken>(
            Builders<RefreshToken>.IndexKeys.Ascending(r => r.ExpiresAt),
            new CreateIndexOptions { Name = "ttl_refresh_expiresAt", ExpireAfter = TimeSpan.Zero });
        return col.Indexes.CreateManyAsync(new[] { hashUnique, ttl }, ct);
    }
}
