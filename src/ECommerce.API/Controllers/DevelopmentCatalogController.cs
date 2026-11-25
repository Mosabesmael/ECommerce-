using System.Security.Claims;
using ECommerce.Application.DTOs;
using ECommerce.Application.Services;
using ECommerce.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/development/catalog")]
[Route("api/admin/catalog")]
[DevelopmentCatalog(allowAdministrator: true)]
public sealed class DevelopmentCatalogController(CatalogManagementService service, BrandService brands) : ControllerBase
{
    [HttpGet("access")]
    public IActionResult CheckAccess() => NoContent();

    [HttpPost("categories")]
    public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await service.CreateCategoryAsync(request, cancellationToken);
        return CreatedAtAction(nameof(CategoriesController.GetById), "Categories", new { id = category.Id }, category);
    }

    [HttpPost("brands")]
    public async Task<ActionResult<BrandDto>> CreateBrand(CreateBrandRequest request, CancellationToken cancellationToken)
    {
        var brand = await brands.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(BrandsController.GetById), "Brands", new { id = brand.Id }, brand);
    }

    [HttpPost("products")]
    public async Task<ActionResult<ManagedProductDto>> CreateProduct(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await service.CreateProductAsync(request, cancellationToken);
        return Created($"{Request.PathBase}{Request.Path}/{product.Id}", product);
    }

    [HttpGet("products/{id:int}")]
    public async Task<ActionResult<ManagedProductDto>> GetProduct(int id, CancellationToken cancellationToken)
    {
        var product = await service.GetProductAsync(id, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPut("products/{id:int}")]
    public async Task<ActionResult<ManagedProductDto>> UpdateProduct(int id, UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await service.UpdateProductAsync(id, request, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }
}

internal sealed class DevelopmentCatalogAttribute(bool allowAdministrator = false) : Attribute, IResourceFilter, IExceptionFilter
{
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        var http = context.HttpContext;
        var environment = http.RequestServices.GetRequiredService<IWebHostEnvironment>();
        if (!http.Request.Path.StartsWithSegments("/api/admin"))
        {
            if (!environment.IsDevelopment()) context.Result = new NotFoundResult();
            return;
        }

        if (!allowAdministrator || http.User.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var configuration = http.RequestServices.GetRequiredService<IConfiguration>();
        if (!Guid.TryParse(configuration["CatalogAdmin:CustomerId"], out var adminId) || adminId == Guid.Empty ||
            !Guid.TryParse(http.User.FindFirstValue(ClaimTypes.NameIdentifier), out var customerId) || customerId != adminId)
        {
            context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
        }
    }

    public void OnResourceExecuted(ResourceExecutedContext context) { }

    public void OnException(ExceptionContext context)
    {
        var status = context.Exception switch
        {
            CatalogWriteConflictException => StatusCodes.Status409Conflict,
            DomainException => StatusCodes.Status400BadRequest,
            _ => (int?)null
        };

        if (status is int code)
        {
            context.Result = new ObjectResult(new ProblemDetails { Status = code, Title = context.Exception.Message })
            {
                StatusCode = code
            };
        }
    }
}
