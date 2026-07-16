using MongoDB.Driver;
using VoiceFlowStudio.Core.Entities;
using VoiceFlowStudio.Core.Interfaces;
using VoiceFlowStudio.Infrastructure.Persistence;

namespace VoiceFlowStudio.Infrastructure.Repositories;

public sealed class ProjectRepository : IProjectRepository
{
    private readonly IMongoCollection<Project> _col;

    public ProjectRepository(IMongoClientFactory factory)
    {
        _col = factory.GetDatabase().GetCollection<Project>("projects");
    }

    public async Task<IReadOnlyList<Project>> ListByIdsAsync(
        IReadOnlyCollection<string> ids,
        IReadOnlyCollection<string>? fields = null,
        CancellationToken ct = default)
    {
        if (ids.Count == 0) return Array.Empty<Project>();

        var filter = Builders<Project>.Filter.In(p => p.Id, ids);
        var find = _col.Find(filter).SortByDescending(p => p.CreatedAt);
        if (fields is { Count: > 0 })
        {
            var proj = Builders<Project>.Projection;
            ProjectionDefinition<Project>? combined = null;
            foreach (var f in fields)
                combined = combined is null ? proj.Include(f) : combined.Include(f);
            return await find.Project<Project>(combined!).ToListAsync(ct);
        }
        return await find.ToListAsync(ct);
    }

    public Task<Project?> GetByIdAsync(string id, CancellationToken ct = default) =>
        _col.Find(p => p.Id == id).FirstOrDefaultAsync(ct)!;

    public Task InsertAsync(Project project, CancellationToken ct = default) =>
        _col.InsertOneAsync(project, cancellationToken: ct);

    public async Task<bool> UpdateAsync(Project project, CancellationToken ct = default)
    {
        var update = Builders<Project>.Update
            .Set(p => p.Name, project.Name)
            .Set(p => p.Description, project.Description)
            .Set(p => p.Color, project.Color)
            .Set(p => p.UpdatedAt, project.UpdatedAt);
        var result = await _col.UpdateOneAsync(p => p.Id == project.Id, update, cancellationToken: ct);
        return result.MatchedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var r = await _col.DeleteOneAsync(p => p.Id == id, ct);
        return r.DeletedCount > 0;
    }
}
