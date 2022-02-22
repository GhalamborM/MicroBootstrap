namespace MicroBootstrap.Abstractions.Messaging.Serialization;

public interface IMessageSerializer
{
    bool CanHandle(Type type);
    string Serialize(object obj, bool camelCase = true, bool indented = true);
    T? Deserialize<T>(string payload, bool camelCase = true);
    object? Deserialize(string payload, Type type, bool camelCase = true);
}
