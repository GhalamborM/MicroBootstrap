using MicroBootstrap.Abstractions.Persistence.EventStore;
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
    public IAggregateStore AggregateStore => _provider.GetRequiredService<IAggregateStore>();

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