namespace MicroBootstrap.Persistence.EventStoreDB;

public class EventStoreDbOptions
{
    public bool UseInternalCheckpointing { get; set; } = true;
    public string ConnectionString { get; set; } = default!;
}
