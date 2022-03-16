using System.Reflection;
using MicroBootstrap.Core.Persistence.EfCore;
using Microsoft.EntityFrameworkCore;

namespace MicroBootstrap.Scheduling.Internal.Data;

public class InternalMessageDbContext : EfDbContextBase
{
    /// <summary>
    /// The default database schema.
    /// </summary>
    public const string DefaultSchema = "messaging";

    public DbSet<InternalMessage> InternalMessages => Set<InternalMessage>();

    public InternalMessageDbContext(DbContextOptions<InternalMessageDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension(EfConstants.UuidGenerator);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}
