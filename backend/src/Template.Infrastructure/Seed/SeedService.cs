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
    Task SeedPositionsIfNeededAsync(CancellationToken ct = default);
    Task SeedAllAsync(CancellationToken ct = default);
}

public sealed class SeedService : ISeedService
{
    private readonly IUserRepository _users;
    private readonly IPositionRepository _positions;
    private readonly IExamTemplateRepository _templates;
    private readonly IPasswordHasher _hasher;
    private readonly SeedOptions _options;
    private readonly ILogger<SeedService> _logger;

    public SeedService(
        IUserRepository users,
        IPositionRepository positions,
        IExamTemplateRepository templates,
        IPasswordHasher hasher,
        IOptions<SeedOptions> options,
        ILogger<SeedService> logger)
    {
        _users = users;
        _positions = positions;
        _templates = templates;
        _hasher = hasher;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SeedAllAsync(CancellationToken ct = default)
    {
        await SeedSuperAdminIfNeededAsync(ct);
        await SeedPositionsIfNeededAsync(ct);
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

    public async Task SeedPositionsIfNeededAsync(CancellationToken ct = default)
    {
        var existing = await _positions.GetAllAsync(ct);
        if (existing.Count > 0)
            return;

        var superAdmin = await _users.GetByEmailAsync(_options.SuperAdminEmail.Trim().ToLowerInvariant(), ct);
        var createdBy = superAdmin?.Id ?? string.Empty;
        var now = DateTime.UtcNow;

        foreach (var seed in PositionSeedCatalog.Defaults)
        {
            var position = new Position
            {
                Name = seed.Name,
                Description = seed.Description,
                IsActive = true,
                CreatedBy = createdBy,
            };
            await _positions.InsertAsync(position, ct);

            var partitions = seed.Partitions.Select((seedPartition, partitionOrder) => new TemplatePartition
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = seedPartition.Name,
                Order = partitionOrder,
                Questions = seedPartition.Questions.Select(q => new TemplateQuestion
                {
                    Id = q.Id,
                    Type = q.Type,
                    Text = q.Text,
                    Points = q.Points,
                    CorrectAnswer = q.CorrectAnswer,
                    Choices = q.Choices?.Select(c => new McqChoice { Id = c.Id, Text = c.Text }).ToList(),
                    CorrectChoiceId = q.CorrectChoiceId,
                    Order = q.Order,
                }).ToList(),
            }).ToList();

            var questionCount = partitions.Sum(p => p.Questions.Count);

            await _templates.InsertAsync(new ExamTemplate
            {
                PositionId = position.Id,
                DurationMinutes = seed.DurationMinutes,
                Partitions = partitions,
                LastModifiedAt = now,
                LastModifiedBy = createdBy,
            }, ct);

            _logger.LogInformation(
                "Seeded position {Name} with {PartitionCount} partitions and {QuestionCount} questions.",
                seed.Name,
                partitions.Count,
                questionCount);
        }
    }
}
