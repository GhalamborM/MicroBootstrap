using FluentAssertions;
using MicroBootstrap.Abstractions.Core.Domain.Events.Store;
using MicroBootstrap.Core.Extensions.DependencyInjection;
using MicroBootstrap.Core.Persistence.EventStore.InMemory;
using MicroBootstrap.Tests.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MicroBootstrap.Core.Tests;

public class DependencyInjectionsTests
{
    private readonly ServiceProvider _provider;

    public DependencyInjectionsTests()
    {
        var services = new ServiceCollection();
        var configuration = ConfigurationHelper.BuildConfiguration();

        services.AddCore(configuration);

        _provider = services.BuildServiceProvider();
    }


    [Fact]
    public void should_resolve_inmemory_event_store() {
        _provider.GetService<IEventStore>().Should().NotBeNull();
        _provider.GetService<IEventStore>().Should().BeOfType<InMemoryEventStore>();
    }
}