using Compass.Modules.Calendar.Domain.Exceptions;

namespace Compass.Modules.Calendar.Domain.Model;

public sealed record TimeZoneId
{
    public string Value { get; }

    public TimeZoneId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new CalendarDomainException("O identificador de fuso horário não pode ser nulo ou vazio.");
        }

        var trimmed = value.Trim();

        try
        {
            _ = TimeZoneInfo.FindSystemTimeZoneById(trimmed);
        }
        catch (Exception ex)
        {
            throw new CalendarDomainException($"Identificador de fuso horário inválido ou não reconhecido: '{trimmed}'.", ex);
        }

        Value = trimmed;
    }

    public TimeZoneInfo ToTimeZoneInfo() => TimeZoneInfo.FindSystemTimeZoneById(Value);

    public override string ToString() => Value;

    public static implicit operator string(TimeZoneId timeZoneId) => timeZoneId.Value;
    public static explicit operator TimeZoneId(string value) => new(value);
}
