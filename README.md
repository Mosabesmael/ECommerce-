# ECommerce API

An ASP.NET Core learning project with a SQL Server catalog, customer accounts, saved addresses, carts, coupons, orders, and verified-purchase reviews. Checkout uses a local payment simulator.

## Run locally

Use the .NET 10 SDK, SQL Server or LocalDB, and the `dotnet-ef` 10.x tool. Set `ConnectionStrings__DefaultConnection` for your development database before applying migrations. For example, in PowerShell with LocalDB:

```powershell
$env:ConnectionStrings__DefaultConnection = 'Server=(localdb)\MSSQLLocalDB;Database=ECommerceLearning;Integrated Security=true;TrustServerCertificate=true'
dotnet ef database update --project src/ECommerce.Infrastructure --startup-project src/ECommerce.API
dotnet run --project src/ECommerce.API --launch-profile http
```

On Windows, `./scripts/Start-Store.ps1` builds the app, applies migrations to the local `ECommerceLearningWeb` database, and starts the storefront and API together at `http://localhost:5274`. It adds sample products to an empty catalog. Set `RIWAQ_SKIP_DEMO_DATA=1` in the environment to skip this step. The script prints the server process ID for stopping it when finished.

To debug in Visual Studio, stop any instance already using port `5274`. Set `ECommerce.API` as the startup project, select the `http` profile, and press **F5**. The same instance serves the storefront at `/` and the HTTP API at `/api`. On a fresh setup, run the start script once to prepare and seed LocalDB, then stop its server process before starting the debugger.

On a fresh database, use the development catalog requests in [the HTTP file](src/ECommerce.API/ECommerce.API.http) to create a category and a product. Copy the returned `id` values into the variables at the top. These catalog management routes are available in the `Development` environment.

To edit a product, read it from the development catalog route and copy its `version` into the update request. Send all product fields, including `stockQuantity` and `isActive`. A `409 Conflict` after checkout or another catalog edit means you should read the current product before retrying.

## Try the customer journey

Use [the sample HTTP requests](src/ECommerce.API/ECommerce.API.http). Create a category, brand, and product, then register a customer and log in. Copy the returned `accessToken` to `AccessToken`. Save a sample address, create a cart with the same token, and copy the returned IDs into the request variables. Use fictitious customer and address data when practicing locally.

- Create an optional percentage coupon through the development endpoint. During checkout the server validates its expiration and calculates the discount.
- `approvePayment: false` simulates a decline and leaves stock unchanged. `approvePayment: true` saves an order, including a shipping address snapshot and a demo payment reference, and deducts stock in the same database save.
- Repeating a successful checkout returns the original order. The authenticated customer can list purchases at `GET /api/orders` and create or edit their own review of a purchased product.
- Guest carts also work. Their random UUID grants access to the cart and its order. Signed-in carts use account ownership and require a saved shipping address at checkout.

The simulator uses a Boolean input and generates its payment reference locally.

## Run the integration smoke test

With LocalDB and `sqlcmd` installed on Windows, run `./scripts/Test-LearningStore.ps1` from PowerShell. The script creates a temporary database, verifies the upgrade and customer journey through HTTP, then removes the test database.

## Catalog administrator

Register the intended administrator with `POST /api/auth/register` and copy the customer `id` from the response. Configure `CatalogAdmin__CustomerId` on the server with that ID. For local PowerShell, set `$env:CatalogAdmin__CustomerId = '<customer-id>'` before running the start script. Sign into the storefront with that account and open **إدارة المنتجات** from **حسابي**. Products created by the administrator use a placeholder unless a built-in demo photo matches their slug.

## Hosting

The frontend and ASP.NET Core API publish together. Use a .NET 10 web host such as Azure App Service and an external SQL Server database such as Azure SQL Database. Keep the hosting environment set to `Production` so the development-only HTTP routes stay closed.

1. Create a database and configure `ConnectionStrings__DefaultConnection` in the host's app settings. Keep the connection string out of the repository. On Azure App Service, you can alternatively configure a SQL Server connection string named `DefaultConnection`.
2. Apply EF migrations to the hosted database from a machine with database access. Set `ConnectionStrings__DefaultConnection` for that shell, then run `dotnet ef database update --project src/ECommerce.Infrastructure --startup-project src/ECommerce.API`.
3. Publish `src/ECommerce.API/ECommerce.API.csproj` in Release from Visual Studio to the App Service. The `wwwroot` storefront ships in the same publish output. Use the App Service HTTPS URL to open the store.
4. Register the admin account, configure the account as above, then use the admin page to populate the hosted catalog. Payment stays simulated.
