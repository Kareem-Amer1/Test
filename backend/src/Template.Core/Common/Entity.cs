using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HireExam.Core.Common;

/// <summary>
/// Base entity. All IDs are <see cref="string"/> exposed in C# but stored as
/// ObjectId in BSON. Mapping is declared via MongoDB data annotations on the
/// entity itself (no BsonClassMap registration required).
/// </summary>
public abstract class Entity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
