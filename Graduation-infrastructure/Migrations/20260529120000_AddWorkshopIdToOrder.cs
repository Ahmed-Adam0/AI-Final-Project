using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation_infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkshopIdToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WorkshopId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_WorkshopId",
                table: "Orders",
                column: "WorkshopId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Workshops_WorkshopId",
                table: "Orders",
                column: "WorkshopId",
                principalTable: "Workshops",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Workshops_WorkshopId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_WorkshopId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "WorkshopId",
                table: "Orders");
        }
    }
}
