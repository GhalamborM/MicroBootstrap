using BuildingBlocks.Tests.Integration.Helpers;
using EventStore.Client;
using FluentAssertions;
using MicroBootstrap.Persistence.EventStoreDB.Extensions;
using MicroBootstrap.Tests.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace MicroBootstrap.Persistence.EventStoreDB.Tests;

public class DependencyInjectionsTests
{
    private readonly ServiceProvider _provider;

    public DependencyInjectionsTests()
    {
        var services = new ServiceCollection();
        var configuration = ConfigurationHelper.BuildConfiguration();

        services.AddEventStoreDb(configuration);

        _provider = services.BuildServiceProvider();
    }

    [Fact]
    public void should_resolve_event_store_db()
    {
        var eventStoreClient = _provider.GetService<EventStoreClient>();
        eventStoreClient.Should().NotBeNull();
    }

    [Fact]
    public void should_resolve_event_store_db_options()
    {
        var options = _provider.GetService<IOptions<EventStoreDbOptions>>();
        options.Should().NotBeNull();
        options.Value.ConnectionString.Should().Be("esdb://localhost:2113?tls=false");
    }
}