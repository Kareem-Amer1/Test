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

    public Task InsertAsync(User user, CancellationToken ct = default) =>
        _col.InsertOneAsync(user, cancellationToken: ct);

    public async Task<bool> ExistsWithRoleAsync(string role, CancellationToken ct = default)
    {
        var count = await _col.CountDocumentsAsync(u => u.Role == role, cancellationToken: ct);
        return count > 0;
    }
}
