using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation_infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIdToProductMaterialOption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductMaterialOptions",
                table: "ProductMaterialOptions");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "ProductMaterialOptions",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductMaterialOptions",
                table: "ProductMaterialOptions",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ProductMaterialOptions_ProductId",
                table: "ProductMaterialOptions",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductMaterialOptions",
                table: "ProductMaterialOptions");

            migrationBuilder.DropIndex(
                name: "IX_ProductMaterialOptions_ProductId",
                table: "ProductMaterialOptions");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ProductMaterialOptions");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductMaterialOptions",
                table: "ProductMaterialOptions",
                columns: new[] { "ProductId", "VendorMaterialOptionId", "PriceOption" });
        }
    }
}
