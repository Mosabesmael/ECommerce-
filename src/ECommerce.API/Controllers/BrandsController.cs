using ECommerce.Application.DTOs;
using ECommerce.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class BrandsController(BrandService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BrandDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await service.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BrandDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var brand = await service.GetByIdAsync(id, cancellationToken);
        return brand is null ? NotFound() : Ok(brand);
    }
}
