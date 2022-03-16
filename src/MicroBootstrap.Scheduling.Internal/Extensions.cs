using System.Reflection;
using Ardalis.GuardClauses;
using MicroBootstrap.Abstractions.Scheduler;
using MicroBootstrap.Core.Extensions.Configuration;
using MicroBootstrap.Core.Persistence.EfCore;
using MicroBootstrap.Scheduling.Internal.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MicroBootstrap.Scheduling.Internal;

public static class Extensions
{
    public static IServiceCollection AddInternalScheduler<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly migrationAssembly)
        where TContext : EfDbContextBase
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        services.AddOptions<InternalMessageSchedulerOptions>().Bind(configuration.GetSection(nameof(InternalMessageSchedulerOptions)))
            .ValidateDataAnnotations();

        services.AddDbContext<TContext>(cfg =>
        {
            var options = Guard.Against.Null(
                configuration.GetOptions<InternalMessageSchedulerOptions>(nameof(InternalMessageSchedulerOptions)),
                nameof(InternalMessageSchedulerOptions));

            cfg.UseNpgsql(options.ConnectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(migrationAssembly.GetName().Name);
                sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            }).UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IScheduler, InternalScheduler>();
        services.AddScoped<IMessageScheduler, InternalScheduler>();
        services.AddScoped<ICommandScheduler, InternalScheduler>();
        services.AddScoped<IInternalSchedulerService, InternalSchedulerService>();

        services.AddHostedService<InternalMessageSchedulerBackgroundWorkerService>();

        return services;
    }
}
