using System.Collections.Immutable;
using FluentAssertions;
using MicroBootstrap.Abstractions.Core.Domain.Events.Store;
using MicroBootstrap.Core.Domain.Events.Internal;
using MicroBootstrap.Core.Domain.Model.EventSourcing;
using MicroBootstrap.Core.Tests.Fixtures;
using Xunit;

namespace MicroBootstrap.Core.Tests;

public class EventStoreRepositoryTests : IClassFixture<IntegrationFixture>
{
    private readonly IntegrationFixture _integrationFixture;

    public EventStoreRepositoryTests(IntegrationFixture fixture)
    {
        _integrationFixture = fixture;
    }

    [Fact]
    public async Task exist_stream_should_return_true_for_existing_stream()
    {
        var shoppingCart = await AddInitItemToStore();

        var exists = await _integrationFixture.EventStoreRepository.Exists<ShoppingCart,Guid>(shoppingCart.Id);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task store_and_get_async_should_return_correct_aggregate()
    {
        var shoppingCart = ShoppingCart.Create(Guid.NewGuid());
        shoppingCart.AddItem(Guid.NewGuid());

        var appendResult = await _integrationFixture.EventStoreRepository.Store<ShoppingCart, Guid>(
            shoppingCart,
            new ExpectedStreamVersion(shoppingCart.OriginalVersion),
            CancellationToken.None);

        var fetchedItem = await _integrationFixture.EventStoreRepository.GetAsync<ShoppingCart, Guid>(shoppingCart.Id);

        appendResult.Should().NotBeNull();
        appendResult.NextExpectedVersion.Should().Be(1);

        fetchedItem.Should().NotBeNull();
        fetchedItem!.Id.Should().Be(shoppingCart.Id);
        fetchedItem.Products.Should().NotBeNullOrEmpty();
        fetchedItem.Products.Count.Should().Be(shoppingCart.Products.Count);
    }

    private async Task<ShoppingCart> AddInitItemToStore()
    {
        var shoppingCart = ShoppingCart.Create(Guid.NewGuid());
        shoppingCart.AddItem(Guid.NewGuid());

        var appendResult = await _integrationFixture.EventStoreRepository.Store<ShoppingCart, Guid>(
            shoppingCart,
            new ExpectedStreamVersion(shoppingCart.OriginalVersion),
            CancellationToken.None);

        return shoppingCart;
    }

    private enum ShoppingCartStatus
    {
        Pending = 1,
        Confirmed = 2,
        Cancelled = 4
    }

    private record ShoppingCartInitialized(Guid ShoppingCartId, Guid ClientId) : DomainEvent;

    private record ProductItemAddedToShoppingCart(Guid ShoppingCartId, Guid ProductId) : DomainEvent;

    private record ProductItemRemovedFromShoppingCart(Guid ShoppingCartId, Guid ProductId) : DomainEvent;

    private record ShoppingCartConfirmed(Guid ShoppingCartId, DateTime ConfirmedAt) : DomainEvent;

    private class ShoppingCart : EventSourcedAggregate<Guid>
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

        internal void Apply(ShoppingCartInitialized @event)
        {
            Id = @event.ShoppingCartId;
            ClientId = @event.ClientId;
            Status = ShoppingCartStatus.Pending;
            _products = new List<Guid>();
        }

        internal void Apply(ProductItemAddedToShoppingCart @event)
        {
            _products.Add(@event.ProductId);
        }

        internal void Apply(ProductItemRemovedFromShoppingCart @event)
        {
            _products.Remove(@event.ProductId);
        }

        internal void Apply(ShoppingCartConfirmed @event)
        {
            ConfirmedAt = @event.ConfirmedAt;
            Status = ShoppingCartStatus.Confirmed;
        }
    }
}