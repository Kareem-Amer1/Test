using MongoDB.Bson.Serialization.Attributes;

namespace HireExam.Core.Entities;

[BsonIgnoreExtraElements]
public sealed class TemplateQuestion
{
    [BsonElement("id")]
    public string Id { get; set; } = string.Empty;

    [BsonElement("type")]
    public string Type { get; set; } = QuestionTypes.Essay;

    [BsonElement("text")]
    public string Text { get; set; } = string.Empty;

    [BsonElement("points")]
    public int Points { get; set; }

    [BsonElement("correctAnswer")]
    public bool? CorrectAnswer { get; set; }

    [BsonElement("choices")]
    public List<McqChoice>? Choices { get; set; }

    [BsonElement("correctChoiceId")]
    public string? CorrectChoiceId { get; set; }

    [BsonElement("order")]
    public int Order { get; set; }
}

[BsonIgnoreExtraElements]
public sealed class McqChoice
{
    [BsonElement("id")]
    public string Id { get; set; } = string.Empty;

    [BsonElement("text")]
    public string Text { get; set; } = string.Empty;
}
