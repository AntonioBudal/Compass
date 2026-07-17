using Compass.Domain.Exceptions;

namespace Compass.Domain.ValueObjects;

public readonly struct CommitmentAttribute : IEquatable<CommitmentAttribute>
{
    public string Key { get; }
    public string Value { get; }

    public CommitmentAttribute(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new DomainException("A chave do atributo não pode ser vazia.");

        Key = key.Trim();
        Value = value?.Trim() ?? string.Empty;
    }

    public bool Equals(CommitmentAttribute other) => 
        string.Equals(Key, other.Key, StringComparison.OrdinalIgnoreCase) && 
        string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is CommitmentAttribute other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Key.ToLowerInvariant(), Value);

    public override string ToString() => $"{Key}: {Value}";
}