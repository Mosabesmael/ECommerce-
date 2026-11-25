namespace ECommerce.Domain.Entities;

public sealed record ShippingAddress(string RecipientName, string Line1, string? Line2,
    string City, string Region, string PostalCode, string CountryCode);
