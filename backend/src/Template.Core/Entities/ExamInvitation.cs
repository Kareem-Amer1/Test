using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using HireExam.Core.Common;

namespace HireExam.Core.Entities;

[BsonIgnoreExtraElements]
public sealed class ExamInvitation : Entity
{
    [BsonElement("token")]
    public string Token { get; set; } = string.Empty;

    [BsonElement("positionId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string PositionId { get; set; } = string.Empty;

    [BsonElement("positionName")]
    public string PositionName { get; set; } = string.Empty;

    [BsonElement("createdBy")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string CreatedBy { get; set; } = string.Empty;

    [BsonElement("createdByName")]
    public string CreatedByName { get; set; } = string.Empty;

    /// <summary>When the invitation link stops accepting new starts/resumes.</summary>
    [BsonElement("linkExpiresAt")]
    public DateTime LinkExpiresAt { get; set; }

    [BsonElement("linkDurationHours")]
    public int LinkDurationHours { get; set; }

    [BsonElement("status")]
    public string Status { get; set; } = InvitationStatuses.Pending;

    [BsonElement("examId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ExamId { get; set; }
}
