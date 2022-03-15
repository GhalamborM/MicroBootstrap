using MicroBootstrap.Messaging.Outbox;
using MicroBootstrap.Persistence.EfCore.Postgres;

namespace MicroBootstrap.Messaging;

public class OutboxDbContextDesignFactory : DbContextDesignFactoryBase<OutboxDataContext>
{
    public OutboxDbContextDesignFactory() : base("PostgresMessaging")
    {
    }
}