using MongoDB.Driver;
using HireExam.Core.Entities;
using HireExam.Core.Interfaces;
using HireExam.Infrastructure.Persistence;

namespace HireExam.Infrastructure.Repositories;

public sealed class ExamInvitationRepository : IExamInvitationRepository
{
    private readonly IMongoCollection<ExamInvitation> _col;

    public ExamInvitationRepository(IMongoClientFactory factory)
    {
        _col = factory.GetDatabase().GetCollection<ExamInvitation>("exam_invitations");
    }

    public Task<ExamInvitation?> GetByTokenAsync(string token, CancellationToken ct = default) =>
        _col.Find(i => i.Token == token).FirstOrDefaultAsync(ct)!;

    public Task<ExamInvitation?> GetByIdAsync(string id, CancellationToken ct = default) =>
        _col.Find(i => i.Id == id).FirstOrDefaultAsync(ct)!;

    public async Task<IReadOnlyList<ExamInvitation>> ListByCreatorAsync(string userId, CancellationToken ct = default)
    {
        var items = await _col.Find(i => i.CreatedBy == userId)
            .SortByDescending(i => i.CreatedAt)
            .ToListAsync(ct);
        return items;
    }

    public async Task<IReadOnlyList<ExamInvitation>> ListAllAsync(CancellationToken ct = default)
    {
        var items = await _col.Find(FilterDefinition<ExamInvitation>.Empty)
            .SortByDescending(i => i.CreatedAt)
            .ToListAsync(ct);
        return items;
    }

    public Task InsertAsync(ExamInvitation invitation, CancellationToken ct = default) =>
        _col.InsertOneAsync(invitation, cancellationToken: ct);

    public async Task<bool> ReplaceAsync(ExamInvitation invitation, CancellationToken ct = default)
    {
        invitation.UpdatedAt = DateTime.UtcNow;
        var r = await _col.ReplaceOneAsync(i => i.Id == invitation.Id, invitation, cancellationToken: ct);
        return r.MatchedCount > 0;
    }
}
