using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CompleteLearningStore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BrandId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CouponCode",
                table: "Orders",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                table: "Orders",
                type: "decimal(28,2)",
                precision: 28,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress_City",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress_CountryCode",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress_Line1",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress_Line2",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress_PostalCode",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress_RecipientName",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress_Region",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "Carts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Brands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Coupons",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: false),
                    PercentOff = table.Column<int>(type: "int", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coupons", x => x.Code);
                    table.CheckConstraint("CK_Coupons_PercentOff", "[PercentOff] BETWEEN 1 AND 100");
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecipientName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Line1 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Line2 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Region = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CountryCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Addresses_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductReviews",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductReviews", x => new { x.ProductId, x.CustomerId });
                    table.ForeignKey(
                        name: "FK_ProductReviews_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductReviews_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_BrandId",
                table: "Products",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId_CreatedAt_Id",
                table: "Orders",
                columns: new[] { "CustomerId", "CreatedAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Carts_CustomerId",
                table: "Carts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_CustomerId",
                table: "Addresses",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Brands_Slug",
                table: "Brands",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_NormalizedEmail",
                table: "Customers",
                column: "NormalizedEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_CustomerId",
                table: "ProductReviews",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_ProductId_CreatedAt_CustomerId",
                table: "ProductReviews",
                columns: new[] { "ProductId", "CreatedAt", "CustomerId" },
                descending: new[] { false, true, false });

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_Customers_CustomerId",
                table: "Carts",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Customers_CustomerId",
                table: "Orders",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Brands_BrandId",
                table: "Products",
                column: "BrandId",
                principalTable: "Brands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carts_Customers_CustomerId",
                table: "Carts");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Customers_CustomerId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Brands_BrandId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "Brands");

            migrationBuilder.DropTable(
                name: "Coupons");

            migrationBuilder.DropTable(
                name: "ProductReviews");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Products_BrandId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CustomerId_CreatedAt_Id",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Carts_CustomerId",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "BrandId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CouponCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Discount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddress_City",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddress_CountryCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddress_Line1",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddress_Line2",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddress_PostalCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddress_RecipientName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddress_Region",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Carts");
        }
    }
}
