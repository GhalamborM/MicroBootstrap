namespace MicroBootstrap.Abstractions.Domain.Model;

public interface IHaveCreator
{
    DateTime Created { get; }
    int? CreatedBy { get; }
}
