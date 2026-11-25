namespace ECommerce.Domain.Entities;

public sealed class Customer
{
    private Customer() { }

    public Customer(string email)
    {
        Id = Guid.NewGuid();
        Email = email.Trim();
        NormalizedEmail = NormalizeEmail(Email);
    }

    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string NormalizedEmail { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;

    public static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();

    public void SetPasswordHash(string passwordHash) => PasswordHash = passwordHash;
}
