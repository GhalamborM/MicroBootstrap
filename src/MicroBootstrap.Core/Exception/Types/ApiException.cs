using System.Net;

namespace MicroBootstrap.Core.Exception.Types;

public class ApiException : CustomException
{
    public ApiException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(message)
    {
        StatusCode = statusCode;
    }
}
