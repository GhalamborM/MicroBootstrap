using System.Reflection;
using Ardalis.GuardClauses;
using MicroBootstrap.Abstractions.Messaging.Outbox;
using MicroBootstrap.Core.Extensions.Configuration;
using MicroBootstrap.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MicroBootstrap.Messaging;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPostgresMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly? migrationAssembly = null)
    {
        AddOutbox(services, configuration, migrationAssembly);

        return services;
    }

    private static void AddOutbox(IServiceCollection services, IConfiguration configuration, Assembly? migrationAssembly)
    {
        var outboxOption = Guard.Against.Null(
            configuration.GetOptions<OutboxOptions>(nameof(OutboxOptions)),
            nameof(OutboxOptions));

        services.AddOptions<OutboxOptions>().Bind(configuration.GetSection(nameof(OutboxOptions)))
            .ValidateDataAnnotations();


        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        services.AddDbContext<OutboxDataContext>(options =>
        {
            options.UseNpgsql(outboxOption.ConnectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly((migrationAssembly ?? Assembly.GetExecutingAssembly()).GetName().Name);
                sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            }).UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IOutboxService, EfOutboxService<OutboxDataContext>>();
    }
}