using MongoDB.Driver;
using VoiceFlowStudio.Core.Entities;
using VoiceFlowStudio.Core.Interfaces;
using VoiceFlowStudio.Infrastructure.Persistence;

namespace VoiceFlowStudio.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _col;

    public UserRepository(IMongoClientFactory factory)
    {
        _col = factory.GetDatabase().GetCollection<User>("users");
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        _col.Find(u => u.Email == email).FirstOrDefaultAsync(ct)!;

    public Task<User?> GetByIdAsync(string id, CancellationToken ct = default) =>
        _col.Find(u => u.Id == id).FirstOrDefaultAsync(ct)!;

    public Task InsertAsync(User user, CancellationToken ct = default) =>
        _col.InsertOneAsync(user, cancellationToken: ct);

    public async Task<bool> AddProjectIdAsync(string userId, string projectId, CancellationToken ct = default)
    {
        var update = Builders<User>.Update
            .AddToSet(u => u.ProjectIds, projectId)
            .Set(u => u.UpdatedAt, DateTime.UtcNow);
        var r = await _col.UpdateOneAsync(u => u.Id == userId, update, cancellationToken: ct);
        return r.MatchedCount > 0;
    }

    public async Task<bool> RemoveProjectIdAsync(string userId, string projectId, CancellationToken ct = default)
    {
        var update = Builders<User>.Update
            .Pull(u => u.ProjectIds, projectId)
            .Set(u => u.UpdatedAt, DateTime.UtcNow);
        var r = await _col.UpdateOneAsync(u => u.Id == userId, update, cancellationToken: ct);
        return r.MatchedCount > 0;
    }
}
