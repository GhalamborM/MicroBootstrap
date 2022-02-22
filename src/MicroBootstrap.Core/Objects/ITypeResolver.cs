namespace MicroBootstrap.Core.Objects;

public interface ITypeResolver
{
    Type Resolve(string typeName);
    void Register(Type type);
    void Register(IList<Type> types);
}
