using MicroBootstrap.Abstractions.Persistence;
using MicroBootstrap.Messaging.Outbox;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MicroBootstrap.Messaging;

public static class ApplicationBuilderExtensions
{
    public static async Task UsePostgresMessaging(
        this IApplicationBuilder app,
        ILogger logger,
        IWebHostEnvironment environment)
    {
        await SeedData(app, logger, environment);
        await ApplyDatabaseMigrations(app, logger);
    }

    private static async Task SeedData(this IApplicationBuilder app, ILogger logger, IWebHostEnvironment environment)
    {
        if (environment.IsEnvironment("test") == false)
        {
            // https://stackoverflow.com/questions/38238043/how-and-where-to-call-database-ensurecreated-and-database-migrate
            // https://www.michalbialecki.com/2020/07/20/adding-entity-framework-core-5-migrations-to-net-5-project/
            using var serviceScope = app.ApplicationServices.CreateScope();
            var seeders = serviceScope.ServiceProvider.GetServices<IDataSeeder>();

            foreach (var seeder in seeders)
            {
                logger.LogInformation("Seeding '{Seed}' started...", seeder.GetType().Name);
                await seeder.SeedAllAsync();
                logger.LogInformation("Seeding '{Seed}' ended...", seeder.GetType().Name);
            }
        }
    }

    public static async Task ApplyDatabaseMigrations(this IApplicationBuilder app, ILogger logger)
    {
        var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
        if (configuration.GetValue<bool>("UseInMemoryDatabase") == false)
        {
            using var serviceScope = app.ApplicationServices.CreateScope();
            var outboxDbContext = serviceScope.ServiceProvider.GetRequiredService<OutboxDataContext>();

            logger.LogInformation("Updating catalog database...");

            await outboxDbContext.Database.MigrateAsync();

            logger.LogInformation("Updated catalog database");
        }
    }
}