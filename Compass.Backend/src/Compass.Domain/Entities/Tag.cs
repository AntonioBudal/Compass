using System.Text.RegularExpressions;
using Compass.Domain.Exceptions;

namespace Compass.Domain.Entities;

public class Tag
{
    private static readonly Regex HexColorRegex = new("^#[a-fA-F0-9]{6}$", RegexOptions.Compiled);

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = null!;
    public string ColorHex { get; private set; } = null!;

    protected Tag() { }

    public Tag(Guid userId, string name, string colorHex = "#6366F1")
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length < 2)
            throw new DomainException("O nome da tag deve possuir pelo menos 2 caracteres.");

        if (string.IsNullOrWhiteSpace(colorHex) || !HexColorRegex.IsMatch(colorHex))
            throw new DomainException("A cor deve estar em formato HEX válido com 7 caracteres (ex: #6366F1).");

        Id = Guid.NewGuid();
        UserId = userId;
        Name = name.Trim().ToLowerInvariant();
        ColorHex = colorHex.ToUpperInvariant();
    }

    public void Update(string name, string colorHex)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length < 2)
            throw new DomainException("O nome da tag deve possuir pelo menos 2 caracteres.");

        if (string.IsNullOrWhiteSpace(colorHex) || !HexColorRegex.IsMatch(colorHex))
            throw new DomainException("A cor deve estar em formato HEX válido.");

        Name = name.Trim().ToLowerInvariant();
        ColorHex = colorHex.ToUpperInvariant();
    }
}