using MicroBootstrap.Abstractions.Core.Domain.Model;

namespace MicroBootstrap.Abstractions.Persistence.Mongo;

public interface IMongoRepository<TEntity, in TId> : IRepository<TEntity, TId>
    where TEntity : class, IHaveIdentity<TId>
{
}
