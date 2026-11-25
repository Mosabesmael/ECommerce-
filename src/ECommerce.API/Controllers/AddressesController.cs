using System.Security.Claims;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Authorize]
[Route("api/addresses")]
public sealed class AddressesController(IAccountRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AddressDto>>> List(CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId))
            return Unauthorized();

        var addresses = await repository.GetAddressesAsync(customerId, cancellationToken);
        return Ok(addresses.Select(AddressDto.FromEntity).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AddressDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId))
            return Unauthorized();

        var address = await repository.GetAddressAsync(customerId, id, cancellationToken);
        return address is null ? NotFound() : Ok(AddressDto.FromEntity(address));
    }

    [HttpPost]
    public async Task<ActionResult<AddressDto>> Create(SaveAddressRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId))
            return Unauthorized();

        var address = new Address(customerId, request.RecipientName, request.Line1, request.Line2, request.City,
            request.Region, request.PostalCode, request.CountryCode);
        await repository.AddAddressAsync(address, cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = address.Id }, AddressDto.FromEntity(address));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AddressDto>> Update(Guid id, SaveAddressRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId))
            return Unauthorized();

        var address = await repository.GetAddressAsync(customerId, id, cancellationToken);
        if (address is null)
            return NotFound();

        address.Update(request.RecipientName, request.Line1, request.Line2, request.City, request.Region,
            request.PostalCode, request.CountryCode);

        return await repository.SaveAddressAsync(cancellationToken) ? Ok(AddressDto.FromEntity(address)) : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId))
            return Unauthorized();

        var address = await repository.GetAddressAsync(customerId, id, cancellationToken);
        if (address is null)
            return NotFound();

        return await repository.RemoveAddressAsync(address, cancellationToken) ? NoContent() : NotFound();
    }

    private bool TryGetCustomerId(out Guid customerId) =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out customerId);
}
