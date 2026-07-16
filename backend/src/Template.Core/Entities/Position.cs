using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using HireExam.Core.Common;

namespace HireExam.Core.Entities;

[BsonIgnoreExtraElements]
public sealed class Position : Entity
{
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("createdBy")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string CreatedBy { get; set; } = string.Empty;
}
