using System.Security.Claims;
using ECommerce.Application.DTOs;
using ECommerce.Application.Services;
using ECommerce.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/carts")]
[CartConflict]
[OptionalCustomer]
public sealed class CartsController(CheckoutService service) : ControllerBase
{
    private Guid? CustomerId => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    [HttpPost]
    public async Task<ActionResult<CartDto>> Create(CancellationToken cancellationToken)
    {
        var cart = await service.CreateAsync(CustomerId, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = cart.Id }, cart);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CartDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var cart = await service.GetAsync(id, CustomerId, cancellationToken);
        return cart is null ? NotFound() : Ok(cart);
    }

    [HttpPut("{id:guid}/items/{productId:int}")]
    public async Task<ActionResult<CartDto>> SetQuantity(Guid id, int productId, SetCartQuantityRequest request,
        CancellationToken cancellationToken)
    {
        var cart = await service.SetQuantityAsync(id, CustomerId, productId, request.Quantity!.Value, cancellationToken);
        return cart is null ? NotFound() : Ok(cart);
    }

    [HttpPost("{id:guid}/demo-checkout")]
    public async Task<ActionResult<OrderDto>> Checkout(Guid id, DemoCheckoutRequest request,
        CancellationToken cancellationToken)
    {
        var order = await service.CheckoutAsync(id, CustomerId, request, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpGet("{id:guid}/order")]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid id, CancellationToken cancellationToken)
    {
        var order = await service.GetOrderAsync(id, CustomerId, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }
}

internal sealed class OptionalCustomerAttribute : Attribute, IResourceFilter
{
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        if (context.HttpContext.Request.Headers.ContainsKey("Authorization") &&
            (context.HttpContext.User.Identity?.IsAuthenticated != true ||
             !Guid.TryParse(context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out _)))
        {
            context.Result = new UnauthorizedResult();
        }
    }

    public void OnResourceExecuted(ResourceExecutedContext context) { }
}

internal sealed class CartConflictAttribute : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        if (context.Exception is DomainException error)
        {
            context.Result = new ConflictObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = error.Message
            });
        }
    }
}
