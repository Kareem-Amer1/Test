using MongoDB.Bson.Serialization.Attributes;

namespace HireExam.Core.Entities;

/// <summary>Named group of questions within an exam template.</summary>
[BsonIgnoreExtraElements]
public sealed class TemplatePartition
{
    [BsonElement("id")]
    public string Id { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("order")]
    public int Order { get; set; }

    [BsonElement("questions")]
    public List<TemplateQuestion> Questions { get; set; } = new();
}
