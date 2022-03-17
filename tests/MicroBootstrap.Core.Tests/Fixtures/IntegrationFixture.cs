using MicroBootstrap.Abstractions.Core.Domain.Events.Store;
using MicroBootstrap.Core.Extensions.DependencyInjection;
using MicroBootstrap.Tests.Shared;
using Microsoft.Extensions.DependencyInjection;
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

        _provider = services.BuildServiceProvider();
    }

    public IEventStore EventStore => _provider.GetRequiredService<IEventStore>();
    public IEventStoreRepository EventSourcedRepository => _provider.GetRequiredService<IEventStoreRepository>();

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