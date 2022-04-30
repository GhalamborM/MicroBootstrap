using Microsoft.EntityFrameworkCore.Infrastructure;

namespace MicroBootstrap.Abstractions.Persistence;

public interface IDbFacadeResolver
{
    DatabaseFacade Database { get; }
}
