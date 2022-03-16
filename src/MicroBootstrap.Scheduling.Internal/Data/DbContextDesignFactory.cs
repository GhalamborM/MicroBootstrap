using MicroBootstrap.Persistence.EfCore.Postgres;

namespace MicroBootstrap.Scheduling.Internal.Data;

public class InternalMessageDbContextDesignFactory : DbContextDesignFactoryBase<InternalMessageDbContext>
{
    public InternalMessageDbContextDesignFactory() : base("InternalMessageConnection")
    {
    }
}