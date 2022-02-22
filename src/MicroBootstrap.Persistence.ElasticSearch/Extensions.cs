using MicroBootstrap.MicroBootstrap.Web.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace MicroBootstrap.MicroBootstrap.Persistence.ElasticSearch;

public static class Extensions
{
    public static void AddElasticsearch(
        this IServiceCollection services, IConfiguration configuration, Action<Nest.ConnectionSettings>? config = null)
    {
        var elasticSearchConfig = configuration.GetOptions<ElasticSearchOptions>(nameof(ElasticSearchOptions));

        var settings = new Nest.ConnectionSettings(new Uri(elasticSearchConfig.Url))
            .DefaultIndex(elasticSearchConfig.DefaultIndex);

        config?.Invoke(settings);

        var client = new ElasticClient(settings);

        services.AddSingleton<IElasticClient>(client);
    }
}
