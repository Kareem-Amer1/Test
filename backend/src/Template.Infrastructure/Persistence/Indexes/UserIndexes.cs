using MongoDB.Driver;
using VoiceFlowStudio.Core.Entities;

namespace VoiceFlowStudio.Infrastructure.Persistence.Indexes;

public static class UserIndexes
{
    public static Task EnsureAsync(IMongoDatabase db, CancellationToken ct = default)
    {
        var col = db.GetCollection<User>("users");
        var emailUnique = new CreateIndexModel<User>(
            Builders<User>.IndexKeys.Ascending(u => u.Email),
            new CreateIndexOptions { Unique = true, Name = "ux_users_email" });
        return col.Indexes.CreateOneAsync(emailUnique, cancellationToken: ct);
    }
}
