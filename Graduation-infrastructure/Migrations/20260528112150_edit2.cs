using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation_infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class edit2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderStatusHistory_Orders_Id",
                table: "OrderStatusHistory");

            migrationBuilder.Sql("UPDATE OrderStatusHistory SET OrderId = Id WHERE OrderId = 0");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistory_OrderId",
                table: "OrderStatusHistory",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderStatusHistory_Orders_OrderId",
                table: "OrderStatusHistory",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderStatusHistory_Orders_OrderId",
                table: "OrderStatusHistory");

            migrationBuilder.DropIndex(
                name: "IX_OrderStatusHistory_OrderId",
                table: "OrderStatusHistory");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderStatusHistory_Orders_Id",
                table: "OrderStatusHistory",
                column: "Id",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
