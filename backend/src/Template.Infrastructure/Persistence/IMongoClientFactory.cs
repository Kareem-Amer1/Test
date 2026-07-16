using MongoDB.Driver;

namespace VoiceFlowStudio.Infrastructure.Persistence;

public interface IMongoClientFactory
{
    IMongoClient Client { get; }
    IMongoDatabase GetDatabase(string? name = null);
}
