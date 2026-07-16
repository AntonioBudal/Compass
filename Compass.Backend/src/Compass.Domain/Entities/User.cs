using Compass.Domain.Exceptions;

namespace Compass.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string TimeZone { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Construtor vazio obrigatório para o EF Core
    protected User() { }

    public User(string email, string name, string passwordHash, string timeZone = "America/Sao_Paulo")
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("O e-mail do usuário não pode ser vazio.");
        
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("O nome do usuário não pode ser vazio.");

        Id = Guid.NewGuid();
        Email = email.Trim().ToLowerInvariant();
        Name = name.Trim();
        PasswordHash = passwordHash;
        TimeZone = timeZone;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateProfile(string name, string timeZone)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("O nome do usuário não pode ser vazio.");

        Name = name.Trim();
        TimeZone = timeZone;
        UpdatedAt = DateTime.UtcNow;
    }
}