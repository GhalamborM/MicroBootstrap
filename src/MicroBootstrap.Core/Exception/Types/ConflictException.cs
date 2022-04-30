using System.Net;

namespace MicroBootstrap.Core.Exception.Types;

public class ConflictException : CustomException
{
    public ConflictException(string message) : base(message)
    {
        StatusCode = HttpStatusCode.Conflict;
    }
}
