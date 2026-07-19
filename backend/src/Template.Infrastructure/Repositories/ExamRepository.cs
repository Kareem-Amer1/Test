using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using HireExam.Core.Entities;
using HireExam.Core.Interfaces;
using HireExam.Infrastructure.Persistence;

namespace HireExam.Infrastructure.Repositories;

public sealed class ExamRepository : IExamRepository
{
    private readonly IMongoCollection<Exam> _col;

    public ExamRepository(IMongoClientFactory factory)
    {
        _col = factory.GetDatabase().GetCollection<Exam>("exams");
    }

    public Task<Exam?> GetByIdAsync(string id, CancellationToken ct = default) =>
        _col.Find(e => e.Id == id).FirstOrDefaultAsync(ct)!;

    public async Task<IReadOnlyList<Exam>> ListAsync(ExamListFilter filter, CancellationToken ct = default)
    {
        var combined = BuildFilter(filter);
        var items = await _col.Find(combined)
            .SortByDescending(e => e.StartedAt)
            .ToListAsync(ct);
        return items;
    }

    public Task<long> CountAsync(ExamListFilter filter, CancellationToken ct = default) =>
        _col.CountDocumentsAsync(BuildFilter(filter), cancellationToken: ct);

    public async Task<IReadOnlyList<PositionExamCount>> CountByPositionAsync(ExamListFilter filter, CancellationToken ct = default)
    {
        var match = BuildFilter(filter);
        var results = await _col.Aggregate()
            .Match(match)
            .Group(
                e => new { e.PositionId, e.PositionName },
                g => new PositionExamCount
                {
                    PositionId = g.Key.PositionId,
                    PositionName = g.Key.PositionName,
                    Count = g.Count(),
                })
            .SortByDescending(x => x.Count)
            .ToListAsync(ct);
        return results;
    }

    public Task InsertAsync(Exam exam, CancellationToken ct = default) =>
        _col.InsertOneAsync(exam, cancellationToken: ct);

    public async Task<bool> ReplaceAsync(Exam exam, CancellationToken ct = default)
    {
        exam.UpdatedAt = DateTime.UtcNow;
        var r = await _col.ReplaceOneAsync(e => e.Id == exam.Id, exam, cancellationToken: ct);
        return r.MatchedCount > 0;
    }

    public async Task<bool> AnyByPositionIdAsync(string positionId, CancellationToken ct = default)
    {
        var count = await _col.CountDocumentsAsync(e => e.PositionId == positionId, cancellationToken: ct);
        return count > 0;
    }

    private static FilterDefinition<Exam> BuildFilter(ExamListFilter filter)
    {
        var builder = Builders<Exam>.Filter;
        var filters = new List<FilterDefinition<Exam>>();

        if (!string.IsNullOrWhiteSpace(filter.ConductedBy))
            filters.Add(builder.Eq(e => e.ConductedBy, filter.ConductedBy));
        if (!string.IsNullOrWhiteSpace(filter.PositionId))
            filters.Add(builder.Eq(e => e.PositionId, filter.PositionId));
        if (!string.IsNullOrWhiteSpace(filter.Status))
            filters.Add(builder.Eq(e => e.Status, filter.Status));
        if (filter.From.HasValue)
            filters.Add(builder.Gte(e => e.StartedAt, filter.From.Value));
        if (filter.To.HasValue)
        {
            var end = filter.To.Value.Date.AddDays(1).AddTicks(-1);
            filters.Add(builder.Lte(e => e.StartedAt, end));
        }
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var escaped = Regex.Escape(filter.Search.Trim());
            var regex = new BsonRegularExpression(escaped, "i");
            filters.Add(builder.Or(
                builder.Regex(e => e.CandidateName, regex),
                builder.Regex(e => e.CandidateEmail, regex),
                builder.Regex(e => e.CandidateMobile, regex)));
        }
        if (filter.PendingGradingOnly)
        {
            filters.Add(builder.Eq(e => e.IsFullyGraded, false));
            filters.Add(builder.Ne(e => e.Status, ExamStatuses.InProgress));
        }

        return filters.Count > 0 ? builder.And(filters) : builder.Empty;
    }
}
