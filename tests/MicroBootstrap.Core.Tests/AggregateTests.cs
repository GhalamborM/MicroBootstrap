using FluentAssertions;
using MicroBootstrap.Abstractions.Domain.Model;
using MicroBootstrap.Core.Domain.Events.Internal;
using Xunit;

namespace MicroBootstrap.Core.Tests;

public class AggregateTests
{
    [Fact]
    public void flush_uncommitted_should_return_all_uncommitted_event_and_clear_this_list()
    {
        var clientId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var shoppingCard = ShoppingCart.Create(clientId);
        shoppingCard.AddItem(productId);
        shoppingCard.Confirm();

        shoppingCard.DomainEvents.Count.Should().Be(3);

        var events = shoppingCard.FlushUncommittedEvents();
        events.Count.Should().Be(3);
        shoppingCard.DomainEvents.Count.Should().Be(0);

        events.Last().Should().BeOfType<ShoppingCartConfirmed>();
        events.Last().AggregateSequenceNumber.Should().Be(2);
        ((Guid)events.Last().AggregateId).Should().Be(shoppingCard.Id);

        // we get current version after commit events with `FlushUncommittedEvents`
        shoppingCard.OriginalVersion.Should().Be(-1);
        shoppingCard.CurrentVersion.Should().Be(2);
    }


    private record ShoppingCartInitialized(Guid ShoppingCartId, Guid ClientId) : DomainEvent;

    private record ProductItemAddedToShoppingCart(Guid ShoppingCartId, Guid ProductId) : DomainEvent;

    private record ProductItemRemovedFromShoppingCart(Guid ShoppingCartId, Guid ProductId) : DomainEvent;

    private record ShoppingCartConfirmed(Guid ShoppingCartId, DateTime ConfirmedAt) : DomainEvent;

    private enum ShoppingCartStatus
    {
        Pending = 1,
        Confirmed = 2,
        Cancelled = 4
    }

    private class ShoppingCart : Aggregate<Guid>
    {
        private List<Guid> _products = new();

        public Guid ClientId { get; private set; }
        public ShoppingCartStatus Status { get; private set; }
        public IReadOnlyList<Guid> Products => _products.AsReadOnly();
        public DateTime? ConfirmedAt { get; private set; }

        public static ShoppingCart Create(Guid clientId)
        {
            var shoppingCart = new ShoppingCart
            {
                ClientId = clientId,
                Id = Guid.NewGuid(),
                _products = new List<Guid>(),
                Status = ShoppingCartStatus.Pending
            };

            shoppingCart.AddDomainEvent(new ShoppingCartInitialized(shoppingCart.Id, shoppingCart.ClientId));

            return shoppingCart;
        }

        public void AddItem(Guid productId)
        {
            _products.Add(productId);

            AddDomainEvent(new ProductItemAddedToShoppingCart(Id, productId));
        }

        public void RemoveItem(Guid productId)
        {
            _products.Remove(productId);

            AddDomainEvent(new ProductItemRemovedFromShoppingCart(Id, productId));
        }

        public void Confirm()
        {
            ConfirmedAt = DateTime.Now;
            Status = ShoppingCartStatus.Confirmed;

            AddDomainEvent(new ShoppingCartConfirmed(Id, DateTime.Now));
        }
    }
}