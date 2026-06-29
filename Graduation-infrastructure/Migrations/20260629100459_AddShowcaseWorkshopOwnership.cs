using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation_infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShowcaseWorkshopOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WorkshopId",
                table: "ShowcaseSlides",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ShowcaseSlides_WorkshopId",
                table: "ShowcaseSlides",
                column: "WorkshopId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShowcaseSlides_Workshops_WorkshopId",
                table: "ShowcaseSlides",
                column: "WorkshopId",
                principalTable: "Workshops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShowcaseSlides_Workshops_WorkshopId",
                table: "ShowcaseSlides");

            migrationBuilder.DropIndex(
                name: "IX_ShowcaseSlides_WorkshopId",
                table: "ShowcaseSlides");

            migrationBuilder.DropColumn(
                name: "WorkshopId",
                table: "ShowcaseSlides");
        }
    }
}
