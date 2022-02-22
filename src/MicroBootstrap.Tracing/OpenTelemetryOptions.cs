namespace MicroBootstrap.Tracing
{
    public class OpenTelemetryOptions
    {
        public IEnumerable<string> Services { get; set; }
        public ZipkinExporterOptions ZipkinExporterOptions { get; set; }
        public JaegerExporterOptions JaegerExporterOptions { get; set; }
        public bool Enabled { get; set; }
        public bool AlwaysOnSampler { get; set; } = true;
        public bool Istio { get; set; }

    }
}