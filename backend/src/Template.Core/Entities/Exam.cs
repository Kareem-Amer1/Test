using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using HireExam.Core.Common;

namespace HireExam.Core.Entities;

/// <summary>Minimal exam entity for position delete validation (Phase 3 expands this).</summary>
[BsonIgnoreExtraElements]
public sealed class Exam : Entity
{
    [BsonElement("positionId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string PositionId { get; set; } = string.Empty;
}
