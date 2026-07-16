using HireExam.Infrastructure.Persistence.Indexes;

namespace HireExam.Infrastructure.Persistence;

public static class CollectionBootstrap
{
    public static async Task EnsureIndexesAsync(IMongoClientFactory factory, CancellationToken ct = default)
    {
        var db = factory.GetDatabase();
        await UserIndexes.EnsureAsync(db, ct);
        await RefreshTokenIndexes.EnsureAsync(db, ct);
        await PositionIndexes.EnsureAsync(db, ct);
        await TemplateIndexes.EnsureAsync(db, ct);
        await ExamIndexes.EnsureAsync(db, ct);
    }
}
