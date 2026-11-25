param([int]$Port = 57244)

$ErrorActionPreference = 'Stop'
$database = 'ECommerceSmoke_' + [guid]::NewGuid().ToString('N')
$server = '(localdb)\MSSQLLocalDB'
$baseUrl = "http://127.0.0.1:$Port"
$previousConnection = $env:ConnectionStrings__DefaultConnection
$previousEnvironment = $env:ASPNETCORE_ENVIRONMENT
$previousAdmin = $env:CatalogAdmin__CustomerId
$env:ConnectionStrings__DefaultConnection = "Server=$server;Database=$database;Integrated Security=true;TrustServerCertificate=true"
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$api = $null

function Invoke-Api($Method, $Path, $Body = $null, $Token = $null, $Expected = 200) {
    $request = @{ Uri = $baseUrl + $Path; Method = $Method; UseBasicParsing = $true; TimeoutSec = 8 }
    if ($null -ne $Body) {
        $request.Body = ConvertTo-Json -InputObject $Body -Compress
        $request.ContentType = 'application/json'
    }
    if ($Token) { $request.Headers = @{ Authorization = "Bearer $Token" } }

    try { $response = Invoke-WebRequest @request }
    catch {
        if ([int]$_.Exception.Response.StatusCode -eq $Expected) { return }
        throw "$Method $Path expected $Expected. $($_.Exception.Message) $($_.ErrorDetails.Message)"
    }
    if ($response.StatusCode -ne $Expected) { throw "$Method $Path expected $Expected, got $($response.StatusCode)." }
    if ($response.Content) { ConvertFrom-Json -InputObject $response.Content }
}

Push-Location (Join-Path $PSScriptRoot '..')
try {
    dotnet build -m:1 -p:UseSharedCompilation=false
    if ($LASTEXITCODE) { throw 'Build failed.' }

    $migrationArgs = @('--project', 'src/ECommerce.Infrastructure', '--startup-project', 'src/ECommerce.API', '--no-build')
    dotnet ef database update AddDemoCheckout @migrationArgs | Select-Object -Last 1
    if ($LASTEXITCODE) { throw 'Previous migration failed.' }
    $guestCartId = [guid]::NewGuid()
    sqlcmd -S $server -E -C -b -d $database -Q "INSERT INTO Carts (Id, Revision) VALUES ('$guestCartId', NEWID());"
    if ($LASTEXITCODE) { throw 'Legacy cart fixture failed.' }
    dotnet ef database update @migrationArgs | Select-Object -Last 1
    if ($LASTEXITCODE) { throw 'Latest migration failed.' }

    $api = Start-Process dotnet -ArgumentList @('src/ECommerce.API/bin/Debug/net10.0/ECommerce.API.dll', '--urls', $baseUrl) -WindowStyle Hidden -PassThru
    $ready = $false
    for ($i = 0; $i -lt 30; $i++) {
        try { $null = Invoke-Api Get '/openapi/v1.json'; $ready = $true; break }
        catch { Start-Sleep -Milliseconds 250 }
    }
    if (!$ready) { throw 'The API did not start.' }
    $null = Invoke-Api Get "/api/carts/$guestCartId"

    $catalog = '/api/development/catalog'
    $category = Invoke-Api Post "$catalog/categories" @{ name = 'Accessories'; slug = 'accessories' } -Expected 201
    $brand = Invoke-Api Post "$catalog/brands" @{ name = 'Demo brand'; slug = 'demo-brand' } -Expected 201
    $product = Invoke-Api Post "$catalog/products" @{
        categoryId = $category.id; brandId = $brand.id; name = 'Keyboard'; slug = 'demo-keyboard'; description = 'Demo';
        sku = 'DEMO-KBD'; price = 50; stockQuantity = 2; isActive = $true
    } -Expected 201
    $matching = Invoke-Api Get "/api/products?brandId=$($brand.id)"
    if ($matching.totalCount -ne 1 -or $matching.items[0].brandName -ne $brand.name) { throw 'Brand filter failed.' }
    $null = Invoke-Api Get "/api/brands/$($brand.id)"
    $null = Invoke-Api Post '/api/development/coupons' @{
        code = 'LEARN10'; percentOff = 10; expiresAt = [DateTimeOffset]::UtcNow.AddDays(1).ToString('o')
    } -Expected 201

    $buyer = @{ email = 'buyer@example.test'; password = 'Local-demo-password-1' }
    $other = @{ email = 'other@example.test'; password = 'Local-demo-password-2' }
    $buyerAccount = Invoke-Api Post '/api/auth/register' $buyer -Expected 201
    $null = Invoke-Api Post '/api/auth/register' $buyer -Expected 409
    $null = Invoke-Api Post '/api/auth/register' $other -Expected 201
    $null = Invoke-Api Post '/api/auth/login' @{ email = $buyer.email; password = 'Wrong-demo-password' } -Expected 401
    $buyerToken = (Invoke-Api Post '/api/auth/login' $buyer).accessToken
    $otherToken = (Invoke-Api Post '/api/auth/login' $other).accessToken
    if (!$buyerToken -or !$otherToken) { throw 'Login failed.' }
    $null = Invoke-Api Get '/api/orders' -Expected 401

    $addressBody = @{
        recipientName = 'Demo Customer'; line1 = 'Demo Street'; city = 'Cairo'; region = 'Cairo'; postalCode = '12345'; countryCode = 'EG'
    }
    $address = Invoke-Api Post '/api/addresses' $addressBody $buyerToken 201
    $otherAddress = Invoke-Api Post '/api/addresses' $addressBody $otherToken 201
    $null = Invoke-Api Get "/api/addresses/$($address.id)" -Token $otherToken -Expected 404

    $cart = Invoke-Api Post '/api/carts' -Token $buyerToken -Expected 201
    $path = '/api/carts/' + $cart.id
    $null = Invoke-Api Get $path -Expected 404
    $null = Invoke-Api Get $path -Token $otherToken -Expected 404
    $null = Invoke-Api Get $path -Token 'invalid-demo-token' -Expected 401
    $null = Invoke-Api Put "$path/items/$($product.id)" @{ quantity = 2 } $buyerToken

    $reviews = "/api/products/$($product.id)/reviews"
    $review = @{ rating = 4; comment = 'Demo review' }
    $null = Invoke-Api Put "$reviews/me" $review $buyerToken 403
    $null = Invoke-Api Post "$path/demo-checkout" @{ approvePayment = $true; addressId = $otherAddress.id } $buyerToken 409
    $null = Invoke-Api Post "$path/demo-checkout" @{ approvePayment = $true; addressId = $address.id; couponCode = 'UNKNOWN' } $buyerToken 409
    $null = Invoke-Api Post "$path/demo-checkout" @{ approvePayment = $false; addressId = $address.id; couponCode = 'LEARN10' } $buyerToken 409
    $null = Invoke-Api Get "$path/order" -Token $buyerToken -Expected 404
    $stock = Invoke-Api Get "$catalog/products/$($product.id)"
    if ($stock.stockQuantity -ne 2) { throw 'Stock changed on decline.' }

    $order = Invoke-Api Post "$path/demo-checkout" @{ approvePayment = $true; addressId = $address.id; couponCode = 'learn10' } $buyerToken
    if ($order.subtotal -ne 100 -or $order.discount -ne 10 -or $order.total -ne 90 -or !$order.demoPaymentReference) {
        throw 'Order totals or simulated payment were incorrect.'
    }
    $retry = Invoke-Api Post "$path/demo-checkout" @{ approvePayment = $true } $buyerToken
    $stock = Invoke-Api Get "$catalog/products/$($product.id)"
    if ($retry.id -ne $order.id -or $stock.stockQuantity -ne 0) { throw 'Checkout was applied more than once.' }
    $addressBody.line1 = 'New Demo Street'
    $null = Invoke-Api Put "/api/addresses/$($address.id)" $addressBody $buyerToken
    $null = Invoke-Api Delete "/api/addresses/$($address.id)" -Token $buyerToken -Expected 204
    $orders = @(Invoke-Api Get '/api/orders' -Token $buyerToken)
    $otherOrders = @(Invoke-Api Get '/api/orders' -Token $otherToken)
    if ($orders.Count -ne 1 -or $orders[0].shippingAddress.line1 -ne 'Demo Street' -or $otherOrders.Count) {
        throw 'Order isolation or shipping address snapshot failed.'
    }

    $null = Invoke-Api Put "$reviews/me" $review $otherToken 403
    $null = Invoke-Api Put "$reviews/me" $review $buyerToken
    $review.rating = 5
    $null = Invoke-Api Put "$reviews/me" $review $buyerToken
    $page = Invoke-Api Get $reviews
    if ($page.totalCount -ne 1 -or $page.items[0].rating -ne 5) { throw 'Review update failed.' }
    Write-Output 'PASS: migrations, guest compatibility, authentication, address and cart ownership, brands, coupons, demo payment, orders, stock, and verified reviews.'

    Stop-Process -Id $api.Id
    $api.WaitForExit()
    $env:ASPNETCORE_ENVIRONMENT = 'Production'
    $env:CatalogAdmin__CustomerId = $buyerAccount.id
    $api = Start-Process dotnet -WorkingDirectory (Join-Path (Get-Location) 'src/ECommerce.API') -ArgumentList @('bin/Debug/net10.0/ECommerce.API.dll', '--urls', $baseUrl) -WindowStyle Hidden -PassThru
    $ready = $false
    for ($i = 0; $i -lt 30; $i++) {
        try { $null = Invoke-Api Get '/api/categories'; $ready = $true; break }
        catch { Start-Sleep -Milliseconds 250 }
    }
    if (!$ready) { throw 'Production API did not start.' }
    $buyerToken = (Invoke-Api Post '/api/auth/login' $buyer).accessToken
    $otherToken = (Invoke-Api Post '/api/auth/login' $other).accessToken
    $admin = '/api/admin/catalog'
    $null = Invoke-Api Get "$admin/access" -Expected 401
    $null = Invoke-Api Get "$admin/access" -Token $otherToken -Expected 403
    $null = Invoke-Api Get "$admin/access" -Token $buyerToken -Expected 204
    $null = Invoke-Api Get "$catalog/access" -Token $buyerToken -Expected 404
    $newProduct = @{ categoryId = $category.id; name = 'Admin sample'; slug = 'admin-sample'; description = 'Demo'; sku = 'ADMIN-KBD'; price = 75; stockQuantity = 1; isActive = $true }
    $null = Invoke-Api Post "$admin/products" $newProduct $otherToken 403
    $created = Invoke-Api Post "$admin/products" $newProduct $buyerToken 201
    $newProduct.version = $created.version
    $newProduct.stockQuantity = 4
    $updated = Invoke-Api Put "$admin/products/$($created.id)" $newProduct $buyerToken
    if ($updated.stockQuantity -ne 4) { throw 'Admin update failed.' }
    $pageResponse = Invoke-WebRequest "$baseUrl/admin.html" -UseBasicParsing -TimeoutSec 6
    $photoResponse = Invoke-WebRequest "$baseUrl/assets/demo-products.png" -UseBasicParsing -TimeoutSec 6
    if ($pageResponse.Content -notmatch '/admin.js' -or $photoResponse.Headers['Content-Type'] -notmatch 'image/png') { throw 'Published admin or photos failed to load.' }
    Write-Output 'PASS: Production administrator authorization, catalog writes, admin page, and product photo asset.'
}
finally {
    if ($api -and !$api.HasExited) { Stop-Process -Id $api.Id }
    sqlcmd -S $server -E -C -b -d master -Q "IF DB_ID('$database') IS NOT NULL BEGIN ALTER DATABASE [$database] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$database]; END"
    $env:ConnectionStrings__DefaultConnection = $previousConnection
    $env:ASPNETCORE_ENVIRONMENT = $previousEnvironment
    $env:CatalogAdmin__CustomerId = $previousAdmin
    Pop-Location
}
