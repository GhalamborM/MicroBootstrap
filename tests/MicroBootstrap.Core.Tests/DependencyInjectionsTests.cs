using FluentAssertions;
using MicroBootstrap.Abstractions.Core.Domain.Events.Store;
using MicroBootstrap.Core.Domain.Events.Store.InMemory;
using MicroBootstrap.Core.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MicroBootstrap.Core.Tests;

public class DependencyInjectionsTests
{
    public DependencyInjectionsTests()
    {
        var services = new ServiceCollection();
        var configuration = InitConfiguration();

        services.AddCore(configuration);

        Provider = services.BuildServiceProvider();
    }

    ServiceProvider Provider { get; }

    [Fact]
    public void should_resolve_inmemory_event_store() {
        Provider.GetService<IEventStore>().Should().NotBeNull();
        Provider.GetService<IEventStore>().Should().BeOfType<InMemoryEventStore>();
    }

    //https://weblog.west-wind.com/posts/2018/Feb/18/Accessing-Configuration-in-NET-Core-Test-Projects
    private static IConfiguration InitConfiguration()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.tests.json")
            .AddEnvironmentVariables()
            .Build();
        return config;
    }
}