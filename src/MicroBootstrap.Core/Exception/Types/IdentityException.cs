using System.Net;

namespace MicroBootstrap.Core.Exception.Types;

public class IdentityException : CustomException
{
    public IdentityException(string message, List<string> errors = default, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        : base(message, statusCode, errors)
    {
    }
}
