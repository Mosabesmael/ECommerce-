using System.Security.Claims;
using ECommerce.Application.DTOs;
using ECommerce.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/products/{productId:int}/reviews")]
public sealed class ProductReviewsController(ProductReviewService reviewService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<ProductReviewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<ProductReviewDto>>> GetPage(int productId,
        [FromQuery] ProductReviewQueryParameters query, CancellationToken cancellationToken)
    {
        var page = await reviewService.GetPageAsync(productId, query, cancellationToken);
        if (page is null)
        {
            return NotFound(new { message = $"Product with id {productId} was not found." });
        }

        return Ok(page);
    }

    [HttpPut("me")]
    [Authorize]
    [ProducesResponseType(typeof(ProductReviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductReviewDto>> Save(int productId, [FromBody] SaveProductReviewRequest request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var customerId) || customerId == Guid.Empty)
        {
            return Unauthorized();
        }

        var result = await reviewService.SaveAsync(productId, customerId, request, cancellationToken);
        return result.Status switch
        {
            SaveProductReviewStatus.ProductNotFound => NotFound(new { message = $"Product with id {productId} was not found." }),
            SaveProductReviewStatus.PurchaseRequired => StatusCode(StatusCodes.Status403Forbidden,
                new { message = "Only customers who purchased this product can review it." }),
            _ => Ok(result.Review)
        };
    }
}
