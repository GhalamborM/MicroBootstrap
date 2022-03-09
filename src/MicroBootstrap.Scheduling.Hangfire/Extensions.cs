using Hangfire;
using Hangfire.PostgreSql;
using MicroBootstrap.Abstractions.Scheduler;
using MicroBootstrap.Scheduling.Hangfire.Scheduler;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace MicroBootstrap.Scheduling.Hangfire;

public static class Extensions
{
    public static IServiceCollection AddHangfireScheduler(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration.GetSection(nameof(HangfireMessageSchedulerOptions)).Get<HangfireMessageSchedulerOptions>();
        services.AddOptions<HangfireMessageSchedulerOptions>().Bind(configuration.GetSection(nameof(HangfireMessageSchedulerOptions)))
            .ValidateDataAnnotations();

        var jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

        services.AddHangfire(hangfireConfiguration =>
        {
            if (options is null || options.UseInMemoryStorage ||
                string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                hangfireConfiguration.UseInMemoryStorage();
            }
            else
            {
                hangfireConfiguration.UsePostgreSqlStorage(options.ConnectionString);
            }

            hangfireConfiguration.UseSerializerSettings(jsonSettings);
        });
        services.AddHangfireServer();

        services.AddScoped<IScheduler, HangfireScheduler>();
        services.AddScoped<IHangfireScheduler, HangfireScheduler>();
        services.AddScoped<ICommandScheduler, HangfireScheduler>();
        services.AddScoped<IMessageScheduler, HangfireScheduler>();

        return services;
    }

    public static IApplicationBuilder UseHangfireScheduler(this IApplicationBuilder app)
    {
        return app.UseHangfireDashboard("/mydashboard");
    }
}
