using FluentAssertions;
using MicroBootstrap.Core.Domain.Events;
using MicroBootstrap.Core.Domain.Events.Internal;
using Xunit;

namespace MicroBootstrap.Core.Tests;

public class TypeMapperTests
{
    [Fact]
    public void get_type_name_should_return_correct_name()
    {
        TypeMapper.GetTypeName<OrderCreated>().Should().Be(typeof(OrderCreated).FullName!.Replace(".", "_"));
    }

    [Fact]
    public void get_type_should_return_correct_type()
    {
        TypeMapper.GetType(nameof(OrderCreated)).Should().Be<OrderCreated>();
    }
}

public record OrderCreated : DomainEvent;