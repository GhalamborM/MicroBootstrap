using MicroBootstrap.Abstractions.Persistence.EventStore;
using MicroBootstrap.Core.Extensions.DependencyInjection;
using MicroBootstrap.CQRS;
using MicroBootstrap.Messaging.InMemory;
using MicroBootstrap.Messaging.Transport.InMemory;
using MicroBootstrap.Persistence.EventStoreDB.Extensions;
using MicroBootstrap.Tests.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace MicroBootstrap.Core.Tests.Fixtures;

public class IntegrationFixture: IAsyncLifetime
{
    private readonly ServiceProvider _provider;

    public IntegrationFixture()
    {
        var services = new ServiceCollection();
        var configuration = ConfigurationHelper.BuildConfiguration();

        services.AddCore(configuration);
        services.AddLogging(builder => { builder.AddXUnit(); });
        services.AddCqrs();
        services.AddInMemoryMessaging(configuration);
        services.AddInMemoryTransport(configuration);
        services.AddEventStoreDb(configuration);
        services.AddHttpContextAccessor();

        _provider = services.BuildServiceProvider();
    }

    public IEventStore EventStore => _provider.GetRequiredService<IEventStore>();

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return Task.CompletedTask;
    }
}