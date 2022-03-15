using Ardalis.GuardClauses;
using MicroBootstrap.Core.Domain.Exceptions;
using MicroBootstrap.Core.Exception;

namespace MicroBootstrap.Core.Domain.ValueObjects;

public record Email
{
    public string Value { get; private set; }

    public static Email? Null => null;

    private Email()
    {
    }

    public static Email Create(string value)
    {
        return new Email
        {
            Value = Guard.Against.InvalidEmail(value, new DomainException($"Email {value} is invalid."))
        };
    }

    public static implicit operator Email?(string? value) => value is null ? null : Create(value);

    public static implicit operator string?(Email? value) => value?.Value;
}
