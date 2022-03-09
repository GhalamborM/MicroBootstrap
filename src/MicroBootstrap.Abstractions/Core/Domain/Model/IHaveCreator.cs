namespace MicroBootstrap.Abstractions.Core.Domain.Model;

public interface IHaveCreator
{
    DateTime Created { get; }
    int? CreatedBy { get; }
}
