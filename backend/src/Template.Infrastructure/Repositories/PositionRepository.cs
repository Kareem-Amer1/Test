using MongoDB.Driver;
using HireExam.Core.Entities;
using HireExam.Core.Interfaces;
using HireExam.Infrastructure.Persistence;

namespace HireExam.Infrastructure.Repositories;

public sealed class PositionRepository : IPositionRepository
{
    private readonly IMongoCollection<Position> _col;

    public PositionRepository(IMongoClientFactory factory)
    {
        _col = factory.GetDatabase().GetCollection<Position>("positions");
    }

    public async Task<IReadOnlyList<Position>> GetAllAsync(CancellationToken ct = default) =>
        await _col.Find(_ => true).SortBy(p => p.Name).ToListAsync(ct);

    public Task<Position?> GetByIdAsync(string id, CancellationToken ct = default) =>
        _col.Find(p => p.Id == id).FirstOrDefaultAsync(ct)!;

    public Task InsertAsync(Position position, CancellationToken ct = default) =>
        _col.InsertOneAsync(position, cancellationToken: ct);

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var r = await _col.DeleteOneAsync(p => p.Id == id, ct);
        return r.DeletedCount > 0;
    }
}

public sealed class ExamTemplateRepository : IExamTemplateRepository
{
    private readonly IMongoCollection<ExamTemplate> _col;

    public ExamTemplateRepository(IMongoClientFactory factory)
    {
        _col = factory.GetDatabase().GetCollection<ExamTemplate>("templates");
    }

    public Task<ExamTemplate?> GetByPositionIdAsync(string positionId, CancellationToken ct = default) =>
        _col.Find(t => t.PositionId == positionId).FirstOrDefaultAsync(ct)!;

    public Task InsertAsync(ExamTemplate template, CancellationToken ct = default) =>
        _col.InsertOneAsync(template, cancellationToken: ct);

    public async Task<bool> ReplaceAsync(ExamTemplate template, CancellationToken ct = default)
    {
        template.UpdatedAt = DateTime.UtcNow;
        var r = await _col.ReplaceOneAsync(t => t.Id == template.Id, template, cancellationToken: ct);
        return r.MatchedCount > 0;
    }

    public async Task<bool> DeleteByPositionIdAsync(string positionId, CancellationToken ct = default)
    {
        var r = await _col.DeleteOneAsync(t => t.PositionId == positionId, ct);
        return r.DeletedCount > 0;
    }
}
