namespace MicroBootstrap.Core.IdsGenerator;

public interface IIdGenerator<out TId>
{
    TId New();
}
