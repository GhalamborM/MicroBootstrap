using System.Diagnostics;

namespace MicroBootstrap.Web.Extensions;

public static class HttpContextExtensions
{
    public static string GetTraceId(this IHttpContextAccessor httpContextAccessor)
    {
        return Activity.Current?.TraceId.ToString() ?? httpContextAccessor?.HttpContext?.TraceIdentifier;
    }

    public static TResult ExtractXQueryObjectFromHeader<TResult>(this HttpContext httpContext, string query)
        where TResult : IPageRequest, new()
    {
        var queryModel = new TResult();
        if (!(string.IsNullOrEmpty(query) || query == "{}"))
        {
            queryModel = JsonConvert.DeserializeObject<TResult>(query);
        }

        httpContext?.Response.Headers.Add("x-query",
            JsonConvert.SerializeObject(queryModel,
                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }));

        return queryModel;
    }
}
