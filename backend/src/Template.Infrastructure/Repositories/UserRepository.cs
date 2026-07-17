using MongoDB.Driver;
using HireExam.Core.Entities;
using HireExam.Core.Interfaces;
using HireExam.Infrastructure.Persistence;

namespace HireExam.Infrastructure.Repositories;

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

    public async Task<IReadOnlyList<User>> ListByRoleAsync(string role, CancellationToken ct = default)
    {
        var items = await _col.Find(u => u.Role == role)
            .SortByDescending(u => u.CreatedAt)
            .ToListAsync(ct);
        return items;
    }

    public Task InsertAsync(User user, CancellationToken ct = default) =>
        _col.InsertOneAsync(user, cancellationToken: ct);

    public async Task<bool> ReplaceAsync(User user, CancellationToken ct = default)
    {
        user.UpdatedAt = DateTime.UtcNow;
        var r = await _col.ReplaceOneAsync(u => u.Id == user.Id, user, cancellationToken: ct);
        return r.MatchedCount > 0;
    }

    public async Task<bool> ExistsWithRoleAsync(string role, CancellationToken ct = default)
    {
        var count = await _col.CountDocumentsAsync(u => u.Role == role, cancellationToken: ct);
        return count > 0;
    }
}
