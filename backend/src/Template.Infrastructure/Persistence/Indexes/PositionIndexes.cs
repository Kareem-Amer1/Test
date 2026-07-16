using MongoDB.Driver;
using HireExam.Core.Entities;

namespace HireExam.Infrastructure.Persistence.Indexes;

public static class PositionIndexes
{
    public static Task EnsureAsync(IMongoDatabase db, CancellationToken ct = default)
    {
        var col = db.GetCollection<Position>("positions");
        var nameIdx = new CreateIndexModel<Position>(
            Builders<Position>.IndexKeys.Ascending(p => p.Name),
            new CreateIndexOptions { Name = "ix_positions_name" });
        return col.Indexes.CreateOneAsync(nameIdx, cancellationToken: ct);
    }
}

public static class TemplateIndexes
{
    public static Task EnsureAsync(IMongoDatabase db, CancellationToken ct = default)
    {
        var col = db.GetCollection<ExamTemplate>("templates");
        var positionUnique = new CreateIndexModel<ExamTemplate>(
            Builders<ExamTemplate>.IndexKeys.Ascending(t => t.PositionId),
            new CreateIndexOptions { Unique = true, Name = "ux_templates_positionId" });
        return col.Indexes.CreateOneAsync(positionUnique, cancellationToken: ct);
    }
}

public static class ExamIndexes
{
    public static Task EnsureAsync(IMongoDatabase db, CancellationToken ct = default)
    {
        var col = db.GetCollection<Exam>("exams");
        var positionIdx = new CreateIndexModel<Exam>(
            Builders<Exam>.IndexKeys.Ascending(e => e.PositionId),
            new CreateIndexOptions { Name = "ix_exams_positionId" });
        var conductorIdx = new CreateIndexModel<Exam>(
            Builders<Exam>.IndexKeys.Ascending(e => e.ConductedBy),
            new CreateIndexOptions { Name = "ix_exams_conductedBy" });
        return Task.WhenAll(
            col.Indexes.CreateOneAsync(positionIdx, cancellationToken: ct),
            col.Indexes.CreateOneAsync(conductorIdx, cancellationToken: ct));
    }
}
