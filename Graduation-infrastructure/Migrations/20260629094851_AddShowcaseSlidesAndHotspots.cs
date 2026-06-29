using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation_infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShowcaseSlidesAndHotspots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShowcaseSlides",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TitleAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitleEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SubtitleAr = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SubtitleEn = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ButtonTextAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ButtonTextEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ButtonLink = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    BackgroundImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShowcaseSlides", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShowcaseHotspots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShowcaseSlideId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    X = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Y = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShowcaseHotspots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShowcaseHotspots_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShowcaseHotspots_ShowcaseSlides_ShowcaseSlideId",
                        column: x => x.ShowcaseSlideId,
                        principalTable: "ShowcaseSlides",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShowcaseHotspots_ProductId",
                table: "ShowcaseHotspots",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ShowcaseHotspots_ShowcaseSlideId",
                table: "ShowcaseHotspots",
                column: "ShowcaseSlideId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShowcaseHotspots");

            migrationBuilder.DropTable(
                name: "ShowcaseSlides");
        }
    }
}
