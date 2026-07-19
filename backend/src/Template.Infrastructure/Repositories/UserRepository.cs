using MongoDB.Driver;
using HireExam.Core.Entities;
using HireExam.Core.Interfaces;
using HireExam.Infrastructure.Persistence;

namespace HireExam.Infrastructure.Repositories;

/// <summary>MongoDB persistence for the <c>users</c> collection.</summary>
public sealed class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _col;

    /// <summary>Resolves the <c>users</c> collection via <see cref="InfrastructureRegistration"/> DI.</summary>
    public UserRepository(IMongoClientFactory factory)
    {
        _col = factory.GetDatabase().GetCollection<User>("users");
    }

    /// <summary>
    /// Looks up a user by normalized email (case-insensitive login key).
    /// Used by <see cref="Application.Services.AuthService.LoginAsync"/> to validate credentials,
    /// and by <see cref="Application.Services.UserService.CreateHrUserAsync"/> to reject duplicate emails.
    /// Also used in <see cref="Seed.SeedService"/> to resolve the Super Admin before seeding positions.
    /// </summary>
    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        _col.Find(u => u.Email == email).FirstOrDefaultAsync(ct)!;

    /// <summary>
    /// Loads a single user by MongoDB ObjectId.
    /// Used by <see cref="Application.Services.AuthService"/> (refresh token, /me, profile update, change password),
    /// <see cref="Application.Services.UserService.DeactivateHrUserAsync"/>, and
    /// <see cref="Application.Services.ExamService.CreateAsync"/> to denormalize <c>conductedByName</c> on new exams.
    /// </summary>
    public Task<User?> GetByIdAsync(string id, CancellationToken ct = default) =>
        _col.Find(u => u.Id == id).FirstOrDefaultAsync(ct)!;

    /// <summary>
    /// Returns all users with the given role, newest first.
    /// Used by <see cref="Application.Services.UserService.ListHrUsersAsync"/> to populate the Super Admin HR accounts page
    /// (<c>GET /api/v1/users</c>).
    /// </summary>
    public async Task<IReadOnlyList<User>> ListByRoleAsync(string role, CancellationToken ct = default)
    {
        var items = await _col.Find(u => u.Role == role)
            .SortByDescending(u => u.CreatedAt)
            .ToListAsync(ct);
        return items;
    }

    /// <summary>
    /// Inserts a new user document (Super Admin seed or HR account creation).
    /// Used by <see cref="Seed.SeedService.SeedSuperAdminIfNeededAsync"/> on first boot and
    /// <see cref="Application.Services.UserService.CreateHrUserAsync"/> (<c>POST /api/v1/users</c>).
    /// </summary>
    public Task InsertAsync(User user, CancellationToken ct = default) =>
        _col.InsertOneAsync(user, cancellationToken: ct);

    /// <summary>
    /// Replaces an existing user document and stamps <c>updatedAt</c>.
    /// Returns <c>false</c> when no document matches the id.
    /// Used by <see cref="Application.Services.AuthService"/> (profile update, change password) and
    /// <see cref="Application.Services.UserService.DeactivateHrUserAsync"/> (soft-delete via <c>isActive = false</c>).
    /// </summary>
    public async Task<bool> ReplaceAsync(User user, CancellationToken ct = default)
    {
        user.UpdatedAt = DateTime.UtcNow;
        var r = await _col.ReplaceOneAsync(u => u.Id == user.Id, user, cancellationToken: ct);
        return r.MatchedCount > 0;
    }

    /// <summary>
    /// Checks whether at least one user exists with the given role (idempotent seed guard).
    /// Used by <see cref="Seed.SeedService.SeedSuperAdminIfNeededAsync"/> to skip creating a second Super Admin.
    /// </summary>
    public async Task<bool> ExistsWithRoleAsync(string role, CancellationToken ct = default)
    {
        var count = await _col.CountDocumentsAsync(u => u.Role == role, cancellationToken: ct);
        return count > 0;
    }
}
