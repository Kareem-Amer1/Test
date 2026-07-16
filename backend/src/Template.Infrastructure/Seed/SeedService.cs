using HireExam.Application.Common;
using HireExam.Application.Validation;
using HireExam.Core.Entities;
using HireExam.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HireExam.Infrastructure.Seed;

public interface ISeedService
{
    Task SeedSuperAdminIfNeededAsync(CancellationToken ct = default);
}

public sealed class SeedService : ISeedService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly SeedOptions _options;
    private readonly ILogger<SeedService> _logger;

    public SeedService(
        IUserRepository users,
        IPasswordHasher hasher,
        IOptions<SeedOptions> options,
        ILogger<SeedService> logger)
    {
        _users = users;
        _hasher = hasher;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SeedSuperAdminIfNeededAsync(CancellationToken ct = default)
    {
        if (await _users.ExistsWithRoleAsync(UserRoles.SuperAdmin, ct))
            return;

        if (!Guard.IsEmail(_options.SuperAdminEmail) || !Guard.IsStrongEnoughPassword(_options.SuperAdminPassword))
        {
            _logger.LogWarning("Seed skipped: SuperAdmin credentials in configuration are invalid.");
            return;
        }

        var user = new User
        {
            Email = _options.SuperAdminEmail.Trim().ToLowerInvariant(),
            PasswordHash = _hasher.Hash(_options.SuperAdminPassword),
            FullName = _options.SuperAdminFullName.Trim(),
            Role = UserRoles.SuperAdmin,
            IsActive = true,
        };

        await _users.InsertAsync(user, ct);
        _logger.LogInformation("Seeded Super Admin account {Email}.", user.Email);
    }
}
