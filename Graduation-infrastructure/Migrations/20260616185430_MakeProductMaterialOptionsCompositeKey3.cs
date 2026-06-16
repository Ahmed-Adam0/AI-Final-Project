using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation_infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeProductMaterialOptionsCompositeKey3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductMaterialOptions",
                table: "ProductMaterialOptions");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductMaterialOptions",
                table: "ProductMaterialOptions",
                columns: new[] { "ProductId", "VendorMaterialOptionId", "PriceOption" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductMaterialOptions",
                table: "ProductMaterialOptions");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductMaterialOptions",
                table: "ProductMaterialOptions",
                columns: new[] { "ProductId", "VendorMaterialOptionId" });
        }
    }
}
