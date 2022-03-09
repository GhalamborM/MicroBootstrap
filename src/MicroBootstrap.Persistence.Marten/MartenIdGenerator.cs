using Marten;
using Marten.Schema.Identity;

namespace MicroBootstrap.Persistence.Marten;

public class MartenIdGenerator : Abstractions.Core.IIdGenerator<Guid>
{
    private readonly IDocumentSession _documentSession;

    public MartenIdGenerator(IDocumentSession documentSession)
    {
        _documentSession = documentSession ?? throw new ArgumentNullException(nameof(documentSession));
    }

    public Guid New() => CombGuidIdGeneration.NewGuid();
}
