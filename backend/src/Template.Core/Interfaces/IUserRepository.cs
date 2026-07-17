using HireExam.Core.Entities;

namespace HireExam.Core.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<User>> ListByRoleAsync(string role, CancellationToken ct = default);
    Task InsertAsync(User user, CancellationToken ct = default);
    Task<bool> ReplaceAsync(User user, CancellationToken ct = default);
    Task<bool> ExistsWithRoleAsync(string role, CancellationToken ct = default);
}
