using MicroBootstrap.Abstractions.Core;

namespace MicroBootstrap.Core.IdsGenerator;

public class GuidIdGenerator : IIdGenerator<Guid>
{
    public Guid New()
    {
       return Guid.NewGuid();
    }
}
