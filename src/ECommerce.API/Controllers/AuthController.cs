using System.Security.Claims;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/auth")]
public sealed class AuthController(IAccountRepository repository, IPasswordHasher<Customer> passwordHasher)
    : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<CustomerDto>> Register(RegisterCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var customer = new Customer(request.Email);
        customer.SetPasswordHash(passwordHasher.HashPassword(customer, request.Password));

        if (!await repository.AddCustomerAsync(customer, cancellationToken))
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "This email is already registered.");

        return StatusCode(StatusCodes.Status201Created, new CustomerDto(customer.Id, customer.Email));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCustomerRequest request, CancellationToken cancellationToken)
    {
        var customer = await repository.FindCustomerAsync(Customer.NormalizeEmail(request.Email), cancellationToken);

        if (customer is null || passwordHasher.VerifyHashedPassword(customer, customer.PasswordHash, request.Password)
            == PasswordVerificationResult.Failed)
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid email or password.");
        }

        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, customer.Id.ToString())],
            BearerTokenDefaults.AuthenticationScheme);

        return SignIn(new ClaimsPrincipal(identity), BearerTokenDefaults.AuthenticationScheme);
    }
}
