using Ardalis.GuardClauses;
using MicroBootstrap.Core.Domain.Exceptions;
using MicroBootstrap.Core.Exception;

namespace MicroBootstrap.Core.Domain.ValueObjects;

public record PhoneNumber
{
    public string Value { get; private set; }

    public static PhoneNumber? Null => null;

    public static PhoneNumber Create(string value)
    {
        return new PhoneNumber
        {
            Value = Guard.Against.InvalidPhoneNumber(
                value,
                new DomainException($"Phone number {value} is invalid."))
        };
    }

    public static implicit operator string?(PhoneNumber? phoneNumber) => phoneNumber?.Value;

    public static implicit operator PhoneNumber?(string? phoneNumber) =>
        phoneNumber == null ? null : Create(phoneNumber);
}
