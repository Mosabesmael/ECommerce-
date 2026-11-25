using ECommerce.Domain.Exceptions;

namespace ECommerce.Domain.Entities;

public sealed class Coupon
{
    public const int MaxCodeLength = 32;
    public string Code { get; private set; } = null!;
    public int PercentOff { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }

    private Coupon() { }

    public static Coupon Create(string code, int percentOff, DateTimeOffset expiresAt, DateTimeOffset now)
    {
        if (percentOff is < 1 or > 100 || expiresAt <= now)
        {
            throw new DomainException("A coupon needs a discount from 1 to 100 percent and a future expiration.");
        }

        return new Coupon { Code = NormalizeCode(code), PercentOff = percentOff, ExpiresAt = expiresAt };
    }

    public static string NormalizeCode(string code)
    {
        code = code.Trim().ToUpperInvariant();
        if (code.Length is 0 or > MaxCodeLength || code.Any(c => !char.IsAsciiLetterOrDigit(c) && c != '-'))
        {
            throw new DomainException("Use 1 to 32 letters, digits or hyphens for the coupon code.");
        }

        return code;
    }
}
