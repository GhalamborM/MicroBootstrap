using Ardalis.GuardClauses;
using MicroBootstrap.Core.Domain.Exceptions;
using MicroBootstrap.Core.Exception;

namespace MicroBootstrap.Core.Domain.ValueObjects;

public record Currency
{
    public string Value { get; private set; }

    public static Currency? Null => null;

    public static Currency Create(string value)
    {
        return new Currency
        {
            Value = Guard.Against.InvalidCurrency(value, new DomainException($"Currency {value} is invalid."))
        };
    }

    public static implicit operator Currency?(string? value) => value == null ? null : Create(value);

    public static implicit operator string?(Currency? value) => value?.Value;
}
