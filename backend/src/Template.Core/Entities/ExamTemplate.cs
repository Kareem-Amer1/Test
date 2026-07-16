using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using HireExam.Core.Common;

namespace HireExam.Core.Entities;

/// <summary>Exam template for a position (collection: templates).</summary>
[BsonIgnoreExtraElements]
public sealed class ExamTemplate : Entity
{
    [BsonElement("positionId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string PositionId { get; set; } = string.Empty;

    [BsonElement("durationMinutes")]
    public int DurationMinutes { get; set; } = 60;

    [BsonElement("questions")]
    public List<TemplateQuestion> Questions { get; set; } = new();

    [BsonElement("lastModifiedAt")]
    public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("lastModifiedBy")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string LastModifiedBy { get; set; } = string.Empty;
}
