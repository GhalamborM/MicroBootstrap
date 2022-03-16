using MicroBootstrap.Messaging.Postgres.Outbox;
using MicroBootstrap.Persistence.EfCore.Postgres;

namespace MicroBootstrap.Messaging.Postgres;

public class OutboxDbContextDesignFactory : DbContextDesignFactoryBase<OutboxDataContext>
{
    public OutboxDbContextDesignFactory() : base("PostgresMessaging")
    {
    }
}