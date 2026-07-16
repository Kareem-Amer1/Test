namespace VoiceFlowStudio.Infrastructure.Persistence;

/// <summary>
/// Multi-database support (Constitution: "MongoClientFactory MUST support
/// resolving multiple named IMongoDatabase instances by name").
/// </summary>
public sealed class MongoOptions
{
    public const string Section = "MongoDB";
    public string ConnectionString { get; set; } = string.Empty;
    /// <summary>Map of logical name -> physical database name.</summary>
    public Dictionary<string, string> Databases { get; set; } = new();
    public string DefaultDatabaseKey { get; set; } = "Default";
}
