namespace MicroBootstrap.Abstractions.Persistence;

public interface IDataSeeder
{
    Task SeedAllAsync();
}
