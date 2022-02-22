namespace MicroBootstrap.Abstractions.Persistence.Mongo;

public interface IMongoUnitOfWork<TContext> : IUnitOfWork<TContext>
    where TContext : MongoDbContext
{
}
