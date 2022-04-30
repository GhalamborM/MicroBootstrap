namespace MicroBootstrap.Messaging.Transport.Rabbitmq;

public class RabbitConfiguration
{
    public string HostName { get; set; }
    public string VirtualHost { get; set; }
    public int Port { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public TimeSpan RetryDelay { get; set; }
}
