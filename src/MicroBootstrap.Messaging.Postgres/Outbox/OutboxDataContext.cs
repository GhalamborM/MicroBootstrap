using System.Reflection;
using MicroBootstrap.Abstractions.Messaging.Outbox;
using MicroBootstrap.Core.Persistence.EfCore;
using Microsoft.EntityFrameworkCore;

namespace MicroBootstrap.Messaging.Postgres.Outbox;

public class OutboxDataContext : EfDbContextBase
{
    /// <summary>
    /// The default database schema.
    /// </summary>
    public const string DefaultSchema = "messaging";

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public OutboxDataContext(DbContextOptions<OutboxDataContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension(EfConstants.UuidGenerator);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}
