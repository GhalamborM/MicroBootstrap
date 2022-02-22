namespace MicroBootstrap.Resiliency.Configs;

public interface IRetryPolicyOptions
{
    int RetryCount { get; set; }
}
