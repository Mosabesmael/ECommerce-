using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();

        services.AddScoped<CheckoutService>();
        services.AddScoped<CatalogManagementService>();
        services.AddScoped<BrandService>();
        services.AddScoped<ProductReviewService>();
        return services;
    }
}
