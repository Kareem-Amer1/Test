using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using VoiceFlowStudio.Core.Common;

namespace VoiceFlowStudio.Core.Entities;

[BsonIgnoreExtraElements]
public sealed class RefreshToken : Entity
{
    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>SHA-256 hash of the opaque refresh token (never store plaintext).</summary>
    [BsonElement("tokenHash")]
    public string TokenHash { get; set; } = string.Empty;

    [BsonElement("expiresAt")]
    public DateTime ExpiresAt { get; set; }

    [BsonElement("revokedAt")]
    public DateTime? RevokedAt { get; set; }

    [BsonIgnore]
    public bool IsActive => RevokedAt is null && DateTime.UtcNow < ExpiresAt;
}
