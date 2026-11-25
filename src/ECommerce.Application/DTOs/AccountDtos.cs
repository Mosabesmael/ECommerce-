using System.ComponentModel.DataAnnotations;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.DTOs;

public sealed record RegisterCustomerRequest(
    [Required, EmailAddress, StringLength(254)] string Email,
    [Required, StringLength(128, MinimumLength = 8)] string Password);

public sealed record LoginCustomerRequest(
    [Required, EmailAddress, StringLength(254)] string Email,
    [Required, StringLength(128)] string Password);

public sealed record CustomerDto(Guid Id, string Email);

public sealed record SaveAddressRequest(
    [Required, StringLength(120)] string RecipientName,
    [Required, StringLength(200)] string Line1,
    [StringLength(200)] string? Line2,
    [Required, StringLength(100)] string City,
    [Required, StringLength(100)] string Region,
    [Required, StringLength(20)] string PostalCode,
    [Required, RegularExpression("[A-Za-z]{2}")] string CountryCode);

public sealed record AddressDto(Guid Id, string RecipientName, string Line1, string? Line2, string City,
    string Region, string PostalCode, string CountryCode)
{
    public static AddressDto FromEntity(Address address) => new(address.Id, address.RecipientName,
        address.Line1, address.Line2, address.City, address.Region, address.PostalCode, address.CountryCode);
}
