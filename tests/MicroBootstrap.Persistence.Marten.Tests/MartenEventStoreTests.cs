using System.Collections.Immutable;
using FluentAssertions;
using MicroBootstrap.Abstractions.Core.Domain.Events.Store;
using MicroBootstrap.Core.Domain;
using MicroBootstrap.Core.Domain.Events.Internal;
using MicroBootstrap.Core.Domain.Model.EventSourcing;
using MicroBootstrap.Core.Persistence.EventStore;
using MicroBootstrap.Core.Persistence.EventStore.Extensions;
using MicroBootstrap.Persistence.Marten.Tests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace MicroBootstrap.Persistence.Marten.Tests;

public class MartenEventStoreTests
{
    private readonly IntegrationFixture _integrationFixture;

    public MartenEventStoreTests(ITestOutputHelper testOutputHelper)
    {
        _integrationFixture = new IntegrationFixture(testOutputHelper);
    }

    [Fact]
    public async Task exist_stream_should_return_true_for_existing_stream()
    {
        var (streamId, _) = await AddInitItemToStore();

        var exists = await _integrationFixture.MartenEventStore.StreamExists(streamId);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task append_multiple_and_get_stream_events_with_version_should_return_correct_events()
    {
        var shoppingCart = ShoppingCart.Create(Guid.NewGuid());
        shoppingCart.AddItem(Guid.NewGuid());
        shoppingCart.Confirm();

        var streamId = StreamName.For<ShoppingCart, Guid>(shoppingCart.Id);

        var uncommittedEvents = shoppingCart.FlushUncommittedEvents();

        var streamEvents = uncommittedEvents
            .Select(x => x.ToStreamEvent(new StreamEventMetadata(x.EventId.ToString(), x.AggregateSequenceNumber)))
            .ToImmutableList();

        var appendResult = await _integrationFixture.MartenEventStore.AppendEventsAsync(
            streamId,
            streamEvents,
            new ExpectedStreamVersion(shoppingCart.OriginalVersion));

        await _integrationFixture.MartenUnitOfWork.CommitAsync(CancellationToken.None);

        var events = (await _integrationFixture.MartenEventStore.GetStreamEventsAsync(
            streamId,
            StreamReadPosition.Start,
            CancellationToken.None)).ToList();

        appendResult.Should().NotBeNull();
        appendResult.NextExpectedVersion.Should().Be(2);
        events.Should().NotBeNull();
        events.Count.Should().Be(3);
        events.All(x => x.Metadata is not null).Should().BeTrue();
        events.Last().Should().BeOfType<StreamEvent<ShoppingCartConfirmed>>();
        events.Last().Metadata!.StreamPosition.Should().Be(2);
    }

    [Fact]
    public async Task append_single_to_existing_stream_and_get_stream_events_with_version_should_return_correct_events()
    {
        await _integrationFixture.MartenUnitOfWork.BeginTransactionAsync();
        var (streamId, _) = await AddInitItemToStore();

        var defaultAggregateState = AggregateFactory<ShoppingCart>.CreateAggregate();

        var aggregate = await _integrationFixture.MartenEventStore.AggregateStreamAsync<ShoppingCart, Guid>(
            streamId,
            StreamReadPosition.Start,
            defaultAggregateState,
            defaultAggregateState.Fold,
            CancellationToken.None
        );

        aggregate!.Confirm();

        var uncommittedEvents = aggregate.FlushUncommittedEvents();

        var streamEvent = uncommittedEvents
            .Select(x => x.ToStreamEvent(new StreamEventMetadata(x.EventId.ToString(), x.AggregateSequenceNumber)))
            .ToImmutableList()
            .First();

        var appendResult = await _integrationFixture.MartenEventStore.AppendEventAsync(
            streamId,
            streamEvent,
            new ExpectedStreamVersion(aggregate.OriginalVersion));

        await _integrationFixture.MartenUnitOfWork.CommitAsync(CancellationToken.None);

        var events = (await _integrationFixture.MartenEventStore.GetStreamEventsAsync(
            streamId,
            StreamReadPosition.Start,
            CancellationToken.None)).ToList();

        appendResult.Should().NotBeNull();
        appendResult.NextExpectedVersion.Should().Be(2);

        events.Should().NotBeNull();
        events.Count.Should().Be(3);
        events.All(x => x.Metadata is not null).Should().BeTrue();
        events.Last().Should().BeOfType<StreamEvent<ShoppingCartConfirmed>>();
        events.Last().Metadata!.StreamPosition.Should().Be(2);

        aggregate.OriginalVersion.Should().Be(1);
        aggregate.CurrentVersion.Should().Be(2);
    }

    [Fact]
    public async Task aggregate_stream_should_return_correct_aggregate()
    {
        var (streamId, shoppingCart) = await AddInitItemToStore();

        var defaultAggregateState = AggregateFactory<ShoppingCart>.CreateAggregate();

        var aggregate = await _integrationFixture.MartenEventStore.AggregateStreamAsync<ShoppingCart, Guid>(
            streamId,
            StreamReadPosition.Start,
            defaultAggregateState,
            defaultAggregateState.Fold,
            CancellationToken.None
        );

        aggregate.Id.Should().Be(shoppingCart.Id);
        aggregate.Products.Count.Should().Be(shoppingCart.Products.Count);

        aggregate.OriginalVersion.Should().Be(1);
        aggregate.CurrentVersion.Should().Be(1);
    }

    private async Task<(StreamName streamName, ShoppingCart shoppingCart)> AddInitItemToStore()
    {
        var shoppingCart = ShoppingCart.Create(Guid.NewGuid());
        shoppingCart.AddItem(Guid.NewGuid());

        var streamId = StreamName.For<ShoppingCart, Guid>(shoppingCart.Id);

        var uncommittedEvents = shoppingCart.FlushUncommittedEvents();

        var streamEvents = uncommittedEvents
            .Select(x => x.ToStreamEvent(new StreamEventMetadata(x.EventId.ToString(), x.AggregateSequenceNumber)))
            .ToImmutableList();

        var appendResult = await _integrationFixture.MartenEventStore.AppendEventsAsync(
            streamId,
            streamEvents,
            new ExpectedStreamVersion(shoppingCart.OriginalVersion));

        return (streamId, shoppingCart);
    }

    public ShoppingCart InitShoppingCart()
    {
        var shoppingCart = ShoppingCart.Create(Guid.NewGuid());

        Enumerable.Range(1, 100)
            .ToList()
            .ForEach(_ => shoppingCart.AddItem(Guid.NewGuid()));

        return shoppingCart;
    }

    public record ShoppingCartInitialized(Guid ShoppingCartId, Guid ClientId) : DomainEvent;

    public record ProductItemAddedToShoppingCart(Guid ShoppingCartId, Guid ProductId) : DomainEvent;

    public record ProductItemRemovedFromShoppingCart(Guid ShoppingCartId, Guid ProductId) : DomainEvent;

    public record ShoppingCartConfirmed(Guid ShoppingCartId, DateTime ConfirmedAt) : DomainEvent;

    public enum ShoppingCartStatus
    {
        Pending = 1,
        Confirmed = 2,
        Cancelled = 4
    }

    public class ShoppingCart : EventSourcedAggregate<Guid>
    {
        private List<Guid> _products = new();

        public Guid ClientId { get; private set; }
        public ShoppingCartStatus Status { get; private set; }
        public IReadOnlyList<Guid> Products => _products.AsReadOnly();
        public DateTime? ConfirmedAt { get; private set; }

        public static ShoppingCart Create(Guid clientId)
        {
            var shoppingCart = new ShoppingCart();

            shoppingCart.ApplyEvent(new ShoppingCartInitialized(Guid.NewGuid(), clientId));

            return shoppingCart;
        }

        public void AddItem(Guid productId)
        {
            ApplyEvent(new ProductItemAddedToShoppingCart(Id, productId));
        }

        public void RemoveItem(Guid productId)
        {
            ApplyEvent(new ProductItemRemovedFromShoppingCart(Id, productId));
        }

        public void Confirm()
        {
            ApplyEvent(new ShoppingCartConfirmed(Id, DateTime.Now));
        }

        public  void Apply(ShoppingCartInitialized @event)
        {
            Id = @event.ShoppingCartId;
            ClientId = @event.ClientId;
            Status = ShoppingCartStatus.Pending;
            _products = new List<Guid>();
        }

        public  void Apply(ProductItemAddedToShoppingCart @event)
        {
            _products.Add(@event.ProductId);
        }

        public  void Apply(ProductItemRemovedFromShoppingCart @event)
        {
            _products.Remove(@event.ProductId);
        }

        public  void Apply(ShoppingCartConfirmed @event)
        {
            ConfirmedAt = @event.ConfirmedAt;
            Status = ShoppingCartStatus.Confirmed;
        }
    }
}