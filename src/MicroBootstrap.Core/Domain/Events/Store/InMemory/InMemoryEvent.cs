using MicroBootstrap.Abstractions.Core.Domain.Events.Store;

namespace MicroBootstrap.Core.Domain.Events.Store.InMemory;

public record InMemoryEvent(IStreamEvent Event, int Position);