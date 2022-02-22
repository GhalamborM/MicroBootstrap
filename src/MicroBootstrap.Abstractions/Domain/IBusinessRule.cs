namespace MicroBootstrap.Abstractions.Domain;

public interface IBusinessRule
{
    bool IsBroken();
    string Message { get; }
}
