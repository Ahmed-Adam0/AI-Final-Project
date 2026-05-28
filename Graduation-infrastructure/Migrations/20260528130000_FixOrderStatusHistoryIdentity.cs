using System;
using Graduation_infrastructure.AppDbContext;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation_infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260528130000_FixOrderStatusHistoryIdentity")]
    public class FixOrderStatusHistoryIdentity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderStatusHistory_Temp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    OldStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NewStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatusHistory_Temp", x => x.Id);
                });

            migrationBuilder.Sql("SET IDENTITY_INSERT OrderStatusHistory_Temp ON");

            migrationBuilder.Sql(
                "INSERT INTO OrderStatusHistory_Temp (Id, OrderId, OldStatus, NewStatus, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsActive) " +
                "SELECT Id, OrderId, OldStatus, NewStatus, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsActive FROM OrderStatusHistory");

            migrationBuilder.Sql("SET IDENTITY_INSERT OrderStatusHistory_Temp OFF");

            migrationBuilder.DropTable(
                name: "OrderStatusHistory");

            migrationBuilder.RenameTable(
                name: "OrderStatusHistory_Temp",
                newName: "OrderStatusHistory");

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderStatusHistory_Orders_OrderId",
                table: "OrderStatusHistory");

            migrationBuilder.DropIndex(
                name: "IX_OrderStatusHistory_OrderId",
                table: "OrderStatusHistory");

            migrationBuilder.CreateTable(
                name: "OrderStatusHistory_Temp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    OldStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NewStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatusHistory_Temp", x => x.Id);
                });

            migrationBuilder.Sql(
                "INSERT INTO OrderStatusHistory_Temp (Id, OrderId, OldStatus, NewStatus, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsActive) " +
                "SELECT Id, OrderId, OldStatus, NewStatus, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsActive FROM OrderStatusHistory");

            migrationBuilder.DropTable(
                name: "OrderStatusHistory");

            migrationBuilder.RenameTable(
                name: "OrderStatusHistory_Temp",
                newName: "OrderStatusHistory");

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
    }
}
