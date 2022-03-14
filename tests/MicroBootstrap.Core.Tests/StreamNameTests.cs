using FluentAssertions;
using MicroBootstrap.Abstractions.Domain.Model;
using MicroBootstrap.Abstractions.Domain.Model.EventSourcing;
using MicroBootstrap.Core.Domain.Events.Store;
using Xunit;

namespace MicroBootstrap.Core.Tests;

public class StreamNameTests
{
    [Fact]
    public void should_return_correct_streamId_value_for_event_sourced_aggregate()
    {
        var id = Guid.NewGuid();
        var streamName = StreamName.For<TestAggregate,Guid>(id);
        streamName.Should().NotBeNull();
        streamName.Value.Should().NotBeNullOrEmpty();
        streamName.Value.Should().Be($"{nameof(TestAggregate)}-{id.ToString()}");
    }

    [Fact]
    public void should_return_correct_streamId_value_for_aggregate()
    {
        var id = Guid.NewGuid();
        var streamName = StreamName.For<TestAggregate2,Guid>(id);
        streamName.Should().NotBeNull();
        streamName.Value.Should().NotBeNullOrEmpty();
        streamName.Value.Should().Be($"{nameof(TestAggregate2)}-{id.ToString()}");
    }
}

public class TestAggregate : EventSourcedAggregate<Guid>
{
}

public class TestAggregate2 : Aggregate<Guid>
{
}