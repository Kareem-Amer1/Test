using MongoDB.Bson.Serialization.Attributes;
using VoiceFlowStudio.Core.Common;

namespace VoiceFlowStudio.Core.Entities;

/// <summary>
/// Project document. Ownership is tracked on the <see cref="User"/> via
/// <c>ProjectIds</c>; the project itself holds no reference back to the user.
/// </summary>
[BsonIgnoreExtraElements]
public sealed class Project : Entity
{
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("color")]
    public string Color { get; set; } = "#3b82f6";
}
