using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using ECommerce.Application.DTOs;
using ECommerce.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Authorize]
[Route("api/orders")]
public sealed class OrdersController(CheckoutService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> Get(CancellationToken cancellationToken,
        [FromQuery, Range(0, int.MaxValue)] int offset = 0, [FromQuery, Range(1, 100)] int limit = 20)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var customerId))
        {
            return Unauthorized();
        }

        return Ok(await service.GetOrdersAsync(customerId, offset, limit, cancellationToken));
    }
}
