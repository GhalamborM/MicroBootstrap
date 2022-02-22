using System.Net;

namespace MicroBootstrap.Resiliency.Retry;

public static class HttpPolicyBuilders
{
    public static PolicyBuilder<HttpResponseMessage> GetBaseBuilder()
    {
        return HttpPolicyExtensions.HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.BadRequest);
    }
}
