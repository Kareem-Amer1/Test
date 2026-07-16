using VoiceFlowStudio.Infrastructure.Persistence.Indexes;

namespace VoiceFlowStudio.Infrastructure.Persistence;

/// <summary>
/// Entity-to-BSON mapping is declared via MongoDB data annotations on the
/// entities themselves (see <c>VoiceFlowStudio.Core.Entities</c>), so no
/// <c>BsonClassMap</c> registration is needed. This class only orchestrates
/// index creation at boot.
/// </summary>
public static class CollectionBootstrap
{
    public static async Task EnsureIndexesAsync(IMongoClientFactory factory, CancellationToken ct = default)
    {
        var db = factory.GetDatabase();
        await UserIndexes.EnsureAsync(db, ct);
        await ProjectIndexes.EnsureAsync(db, ct);
        await RefreshTokenIndexes.EnsureAsync(db, ct);
    }
}
