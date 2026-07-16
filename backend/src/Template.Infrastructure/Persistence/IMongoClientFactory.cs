using MongoDB.Driver;

namespace HireExam.Infrastructure.Persistence;

public interface IMongoClientFactory
{
    IMongoClient Client { get; }
    IMongoDatabase GetDatabase(string? name = null);
}
