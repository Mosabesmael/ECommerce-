using ECommerce.Application;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddAuthentication(BearerTokenDefaults.AuthenticationScheme).AddBearerToken();
builder.Services.AddAuthorization();
builder.Services.AddScoped<IPasswordHasher<Customer>, PasswordHasher<Customer>>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
