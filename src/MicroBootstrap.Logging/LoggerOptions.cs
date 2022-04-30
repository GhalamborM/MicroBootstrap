namespace MicroBootstrap.Logging;

public class LoggerOptions
{
    public string? Level { get; set; }
    public IDictionary<string, string>? MinimumLevelOverrides { get; set; }
    public IEnumerable<string>? ExcludePaths { get; set; }
    public IEnumerable<string>? ExcludeProperties { get; set; }
    public IDictionary<string, object>? Tags { get; set; }
    public bool UseSeq { get; set; }
    public bool UseElasticSearch { get; set; }
    public ElasticSearchLoggingOptions? ElasticSearchLoggingOptions { get; set; }
    public SeqLoggingOptions? SeqOptions { get; set; }
    public string? LogTemplate { get; set; }
    public string? DevelopmentLogPath { get; set; }
    public string? ProductionLogPath { get; set; }
}

public class ElasticSearchLoggingOptions
{
    public string? Url { get; set; }
}

public class SeqLoggingOptions
{
    public string? Url { get; set; }
}