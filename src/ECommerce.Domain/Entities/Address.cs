namespace ECommerce.Domain.Entities;

public sealed class Address
{
    private Address() { }

    public Address(Guid customerId, string recipientName, string line1, string? line2, string city,
        string region, string postalCode, string countryCode)
    {
        Id = Guid.NewGuid();
        CustomerId = customerId;
        Update(recipientName, line1, line2, city, region, postalCode, countryCode);
    }

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public string RecipientName { get; private set; } = null!;
    public string Line1 { get; private set; } = null!;
    public string? Line2 { get; private set; }
    public string City { get; private set; } = null!;
    public string Region { get; private set; } = null!;
    public string PostalCode { get; private set; } = null!;
    public string CountryCode { get; private set; } = null!;

    public void Update(string recipientName, string line1, string? line2, string city, string region,
        string postalCode, string countryCode)
    {
        RecipientName = recipientName.Trim();
        Line1 = line1.Trim();
        Line2 = string.IsNullOrWhiteSpace(line2) ? null : line2.Trim();
        City = city.Trim();
        Region = region.Trim();
        PostalCode = postalCode.Trim();
        CountryCode = countryCode.ToUpperInvariant();
    }
}
