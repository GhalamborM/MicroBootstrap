using Marten;
using MicroBootstrap.Abstractions.Core.Domain.Events.Store;
using MicroBootstrap.Core.Extensions.DependencyInjection;
using MicroBootstrap.CQRS;
using MicroBootstrap.Messaging;
using MicroBootstrap.Messaging.Transport.InMemory;
using MicroBootstrap.Persistence.Marten.Extensions;
using MicroBootstrap.Tests.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace MicroBootstrap.Persistence.Marten.Tests.Fixtures;

public class IntegrationFixture : IAsyncLifetime
{
    private readonly ServiceProvider _provider;

    public IntegrationFixture(ITestOutputHelper outputHelper)
    {
        var services = new ServiceCollection();
        var configuration = ConfigurationHelper.BuildConfiguration();

        services.AddCore(configuration);
        services.AddMarten(configuration);
        services.AddLogging(builder => { builder.AddXUnit(outputHelper); });
        services.AddCqrs();
        services.AddPostgresMessaging(configuration);
        services.AddInMemoryTransport(configuration);
        services.AddHttpContextAccessor();

        _provider = services.BuildServiceProvider();
    }

    public IServiceProvider ServiceProvider => _provider;
    public IEventStore MartenEventStore => _provider.GetRequiredService<IEventStore>();
    public IDocumentStore  DocumentStore  => _provider.GetRequiredService<IDocumentStore>();
    public IDocumentSession  DocumentSession  => _provider.GetRequiredService<IDocumentSession>();
    public IMartenUnitOfWork MartenUnitOfWork => _provider.GetRequiredService<IMartenUnitOfWork>();

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