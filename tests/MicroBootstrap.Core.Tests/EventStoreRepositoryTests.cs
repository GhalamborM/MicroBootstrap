using MicroBootstrap.Core.Tests.Fixtures;
using Xunit;

namespace MicroBootstrap.Core.Tests;

public class EventStoreRepositoryTests : IClassFixture<IntegrationFixture>
{
    private readonly IntegrationFixture _fixture;

    public EventStoreRepositoryTests(IntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task get_async_should_return_correct_aggregate()
    {
    }
}