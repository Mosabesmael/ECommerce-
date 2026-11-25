using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<ProductSummaryDto>>> GetAll(
        [FromQuery] ProductQueryParameters query,
        CancellationToken cancellationToken)
    {
        var products = await _productService.GetActiveProductsAsync(query, cancellationToken);
        return Ok(products);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDetailsDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetProductByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return NotFound(new { message = $"Product with id {id} was not found." });
        }

        return Ok(product);
    }
}
