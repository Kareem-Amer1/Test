using MongoDB.Driver;
using VoiceFlowStudio.Core.Entities;

namespace VoiceFlowStudio.Infrastructure.Persistence.Indexes;

public static class ProjectIndexes
{
    public static Task EnsureAsync(IMongoDatabase db, CancellationToken ct = default)
    {
        // Project no longer carries a userId back-reference; ownership is
        // resolved from User.ProjectIds. A descending createdAt index keeps
        // list-by-ids queries cheap when sorted by recency.
        var col = db.GetCollection<Project>("projects");
        var byCreated = new CreateIndexModel<Project>(
            Builders<Project>.IndexKeys.Descending(p => p.CreatedAt),
            new CreateIndexOptions { Name = "ix_projects_createdAt" });
        return col.Indexes.CreateOneAsync(byCreated, cancellationToken: ct);
    }
}
