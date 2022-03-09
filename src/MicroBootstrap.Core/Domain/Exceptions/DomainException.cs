using System.Net;
using MicroBootstrap.Core.Exception.Types;

namespace MicroBootstrap.Abstractions.Domain.Exceptions;

/// <summary>
/// Exception type for domain exceptions.
/// </summary>
public class DomainException : CustomException
{
    public DomainException(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest) : base(message)
    {
        StatusCode = statusCode;
    }
}
