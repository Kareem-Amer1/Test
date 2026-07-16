using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace HireExam.Infrastructure.Persistence;

public sealed class MongoClientFactory : IMongoClientFactory
{
    private readonly MongoOptions _options;
    public IMongoClient Client { get; }

    public MongoClientFactory(IOptions<MongoOptions> options)
    {
        _options = options.Value;
        if (string.IsNullOrWhiteSpace(_options.ConnectionString))
            throw new InvalidOperationException("Mongo:ConnectionString is not configured.");
        Client = new MongoClient(_options.ConnectionString);
    }

    public IMongoDatabase GetDatabase(string? name = null)
    {
        var key = name ?? _options.DefaultDatabaseKey;
        if (!_options.Databases.TryGetValue(key, out var dbName) || string.IsNullOrWhiteSpace(dbName))
            throw new InvalidOperationException($"Mongo database '{key}' is not configured in Mongo:Databases.");
        return Client.GetDatabase(dbName);
    }
}
