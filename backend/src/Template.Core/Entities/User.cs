using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using VoiceFlowStudio.Core.Common;

namespace VoiceFlowStudio.Core.Entities;

[BsonIgnoreExtraElements]
public sealed class User : Entity
{
    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("passwordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    [BsonElement("role")]
    public string Role { get; set; } = "user";

    /// <summary>
    /// Project ownership lives on the user as a list of project IDs. The
    /// <see cref="Project"/> document intentionally does not carry a back-
    /// reference to the user.
    /// </summary>
    [BsonElement("projectIds")]
    [BsonRepresentation(BsonType.ObjectId)]
    public List<string> ProjectIds { get; set; } = new();
}
