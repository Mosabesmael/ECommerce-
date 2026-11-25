param([ValidateRange(1024, 65535)][int]$Port = 5274)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$apiProject = Join-Path $root 'src/ECommerce.API'
$baseUrl = "http://localhost:$Port"
$env:ASPNETCORE_ENVIRONMENT = 'Development'
if (!$env:ConnectionStrings__DefaultConnection) {
    $env:ConnectionStrings__DefaultConnection = 'Server=(localdb)\MSSQLLocalDB;Database=ECommerceLearningWeb;Integrated Security=true;TrustServerCertificate=true'
}

function Request($Method, $Path, $Body = $null) {
    $parameters = @{ Uri = "$baseUrl$Path"; Method = $Method; TimeoutSec = 10 }
    if ($null -ne $Body) {
        $parameters.Body = ConvertTo-Json -InputObject $Body -Compress
        $parameters.ContentType = 'application/json; charset=utf-8'
    }
    Invoke-RestMethod @parameters
}

Push-Location $root
try {
    dotnet build -m:1 -p:UseSharedCompilation=false
    if ($LASTEXITCODE) { throw 'Build failed.' }
    dotnet ef database update --project src/ECommerce.Infrastructure --startup-project src/ECommerce.API --no-build | Select-Object -Last 2
    if ($LASTEXITCODE) { throw 'Database migration failed.' }

    $api = Start-Process dotnet -WorkingDirectory $apiProject -ArgumentList @('bin/Debug/net10.0/ECommerce.API.dll', '--urls', $baseUrl) -WindowStyle Hidden -PassThru
    try {
        $ready = $false
        for ($attempt = 0; $attempt -lt 25; $attempt++) {
            if ($api.HasExited) { throw "The server exited. Check whether port $Port is already in use." }
            try {
                $homePage = Invoke-WebRequest -Uri $baseUrl -UseBasicParsing -TimeoutSec 3
                if ($homePage.Content -match '/store.js') { $ready = $true; break }
            } catch { Start-Sleep -Milliseconds 300 }
        }
        if (!$ready) { throw 'The storefront did not start.' }

        $catalog = Request Get '/api/products?pageSize=1'
        if ($catalog.totalCount -eq 0 -and !$env:RIWAQ_SKIP_DEMO_DATA) {
            $category = @(Request Get '/api/categories') | Where-Object slug -eq 'desk-accessories' | Select-Object -First 1
            if (!$category) {
                $category = Request Post '/api/development/catalog/categories' @{ name = 'Desk Accessories'; slug = 'desk-accessories' }
            }
            $brand = @(Request Get '/api/brands') | Where-Object slug -eq 'riwaq-studio' | Select-Object -First 1
            if (!$brand) {
                $brand = Request Post '/api/development/catalog/brands' @{ name = 'Riwaq Studio'; slug = 'riwaq-studio' }
            }
            $products = @(
                @{ name = 'Studio Headphones'; slug = 'studio-headphones'; description = 'Wireless over-ear headphones with a soft, adjustable headband.'; sku = 'RIWAQ-001'; price = 1499 },
                @{ name = 'Compact Keyboard'; slug = 'compact-keyboard'; description = 'A compact mechanical keyboard for your desk.'; sku = 'RIWAQ-002'; price = 1250 },
                @{ name = 'Arc Wireless Mouse'; slug = 'arc-wireless-mouse'; description = 'A lightweight wireless mouse with a sculpted grip.'; sku = 'RIWAQ-003'; price = 520 },
                @{ name = 'Desk Light'; slug = 'desk-light'; description = 'A small warm-light lamp for your workspace.'; sku = 'RIWAQ-004'; price = 860 },
                @{ name = 'Everyday USB-C Hub'; slug = 'everyday-usb-c-hub'; description = 'A compact adapter for your everyday devices.'; sku = 'RIWAQ-005'; price = 795 },
                @{ name = 'Felt Desk Mat'; slug = 'felt-desk-mat'; description = 'A soft full-width mat for your keyboard and mouse.'; sku = 'RIWAQ-006'; price = 360 }
            )
            foreach ($product in $products) {
                $product.categoryId = $category.id
                $product.brandId = $brand.id
                $product.stockQuantity = 12
                $product.isActive = $true
                $null = Request Post '/api/development/catalog/products' $product
            }
            Write-Output 'Added six sample products.'
        }

        $items = Request Get '/api/products?pageSize=1'
        Write-Output "Store ready at $baseUrl with $($items.totalCount) active products (PID $($api.Id))."
    }
    catch {
        if (!$api.HasExited) { Stop-Process -Id $api.Id }
        throw
    }
}
finally { Pop-Location }
