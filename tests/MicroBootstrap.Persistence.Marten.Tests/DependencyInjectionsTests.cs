using FluentAssertions;
using MicroBootstrap.Persistence.Marten.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.Abstractions;

namespace MicroBootstrap.Persistence.Marten.Tests;

public class DependencyInjectionsTests
{
    private readonly IntegrationFixture _integrationFixture;

    public DependencyInjectionsTests( ITestOutputHelper outputHelper)
    {
        _integrationFixture = new IntegrationFixture(outputHelper);
    }

    [Fact]
    public void should_resolve_marten_event_store()
    {
        var documentSession = _integrationFixture.MartenEventStore;
        documentSession.Should().NotBeNull();
    }

    [Fact]
    public void should_resolve_marten_document_store()
    {
        var eventStore = _integrationFixture.DocumentStore;
        eventStore.Should().NotBeNull();
    }

    [Fact]
    public void should_resolve_marten_document_session()
    {
        var documentSession = _integrationFixture.DocumentSession;
        documentSession.Should().NotBeNull();
    }

    [Fact]
    public void should_resolve_marten_unit_of_work()
    {
        var martenUnitOfWorkAct = () => _integrationFixture.MartenUnitOfWork;
        martenUnitOfWorkAct.Should().NotThrow();
    }

    [Fact]
    public void should_resolve_event_store_db_options()
    {
        var options = _integrationFixture.ServiceProvider.GetService<IOptions<MartenOptions>>();
        options.Should().NotBeNull();
        options.Value.ConnectionString.Should().NotBeNullOrEmpty();
    }
}