using ECommerce.Application.DTOs;
using ECommerce.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/development/coupons")]
[DevelopmentCatalog]
public sealed class DevelopmentCouponsController(CheckoutService service) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CouponDto>> Create(CreateCouponRequest request, CancellationToken cancellationToken)
    {
        return StatusCode(StatusCodes.Status201Created, await service.CreateCouponAsync(request, cancellationToken));
    }
}
