using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation_infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVendorMaterialsLibrary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VendorMaterialGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkshopId = table.Column<int>(type: "int", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorMaterialGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendorMaterialGroups_Workshops_WorkshopId",
                        column: x => x.WorkshopId,
                        principalTable: "Workshops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VendorMaterialOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendorMaterialGroupId = table.Column<int>(type: "int", nullable: false),
                    ValueAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ValueEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PriceDelta = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorMaterialOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendorMaterialOptions_VendorMaterialGroups_VendorMaterialGroupId",
                        column: x => x.VendorMaterialGroupId,
                        principalTable: "VendorMaterialGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductMaterialOptions",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    VendorMaterialOptionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductMaterialOptions", x => new { x.ProductId, x.VendorMaterialOptionId });
                    table.ForeignKey(
                        name: "FK_ProductMaterialOptions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductMaterialOptions_VendorMaterialOptions_VendorMaterialOptionId",
                        column: x => x.VendorMaterialOptionId,
                        principalTable: "VendorMaterialOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductMaterialOptions_VendorMaterialOptionId",
                table: "ProductMaterialOptions",
                column: "VendorMaterialOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorMaterialGroups_WorkshopId",
                table: "VendorMaterialGroups",
                column: "WorkshopId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorMaterialOptions_VendorMaterialGroupId",
                table: "VendorMaterialOptions",
                column: "VendorMaterialGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductMaterialOptions");

            migrationBuilder.DropTable(
                name: "VendorMaterialOptions");

            migrationBuilder.DropTable(
                name: "VendorMaterialGroups");
        }
    }
}
