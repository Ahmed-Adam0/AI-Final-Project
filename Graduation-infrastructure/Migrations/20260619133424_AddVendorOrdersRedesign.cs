using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation_infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVendorOrdersRedesign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Temp_Orders_OrderId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Workshops_WorkshopId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "OrderStatusHistory");

            migrationBuilder.CreateTable(
                name: "VendorOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MasterOrderId = table.Column<int>(type: "int", nullable: false),
                    WorkshopId = table.Column<int>(type: "int", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendorOrders_Orders_MasterOrderId",
                        column: x => x.MasterOrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VendorOrders_Workshops_WorkshopId",
                        column: x => x.WorkshopId,
                        principalTable: "Workshops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VendorOrderStatusHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendorOrderId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_VendorOrderStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendorOrderStatusHistories_VendorOrders_VendorOrderId",
                        column: x => x.VendorOrderId,
                        principalTable: "VendorOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VendorOrders_MasterOrderId",
                table: "VendorOrders",
                column: "MasterOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorOrders_WorkshopId",
                table: "VendorOrders",
                column: "WorkshopId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorOrderStatusHistories_VendorOrderId",
                table: "VendorOrderStatusHistories",
                column: "VendorOrderId");

            // --- DATA MIGRATION: Populate VendorOrders from existing OrderItems ---
            migrationBuilder.Sql(@"
                WITH RawItems AS (
                    SELECT 
                        oi.OrderId AS MasterOrderId,
                        COALESCE(p.WorkshopId, o.WorkshopId, (SELECT TOP 1 Id FROM Workshops)) AS WorkshopId,
                        oi.SnapshotUnitPrice * oi.Quantity AS ItemTotal,
                        o.Status AS OrderStatus,
                        o.CreatedAt,
                        o.UpdatedAt,
                        o.CreatedBy,
                        o.UpdatedBy,
                        o.IsActive
                    FROM OrderItems oi
                    LEFT JOIN Products p ON oi.ProductId = p.Id
                    INNER JOIN Orders o ON oi.OrderId = o.Id
                )
                INSERT INTO VendorOrders (MasterOrderId, WorkshopId, TotalPrice, Status, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsActive)
                SELECT 
                    MasterOrderId,
                    WorkshopId,
                    SUM(ItemTotal) AS TotalPrice,
                    CASE 
                        WHEN OrderStatus = 'Delivered' THEN 'Delivered'
                        WHEN OrderStatus = 'Cancelled' THEN 'Cancelled'
                        WHEN OrderStatus = 'Shipped' THEN 'Shipped'
                        WHEN OrderStatus = 'Processing' OR OrderStatus = 'In Progress' THEN 'InProgress'
                        ELSE 'Pending'
                    END AS Status,
                    CreatedAt,
                    UpdatedAt,
                    CreatedBy,
                    UpdatedBy,
                    IsActive
                FROM RawItems
                GROUP BY MasterOrderId, WorkshopId, OrderStatus, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsActive
            ");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "OrderItems",
                newName: "VendorOrderId");

            // --- DATA MIGRATION: Link existing OrderItems to the newly created VendorOrders ---
            migrationBuilder.Sql(@"
                UPDATE oi
                SET oi.VendorOrderId = vo.Id
                FROM OrderItems oi
                LEFT JOIN Products p ON oi.ProductId = p.Id
                INNER JOIN Orders o ON oi.VendorOrderId = o.Id
                INNER JOIN VendorOrders vo ON oi.VendorOrderId = vo.MasterOrderId AND COALESCE(p.WorkshopId, o.WorkshopId, (SELECT TOP 1 Id FROM Workshops)) = vo.WorkshopId
            ");

            migrationBuilder.DropIndex(
                name: "IX_Orders_WorkshopId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "WorkshopId",
                table: "Orders");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                newName: "IX_OrderItems_VendorOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_VendorOrders_VendorOrderId",
                table: "OrderItems",
                column: "VendorOrderId",
                principalTable: "VendorOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_VendorOrders_VendorOrderId",
                table: "OrderItems");

            migrationBuilder.DropTable(
                name: "VendorOrderStatusHistories");

            migrationBuilder.DropTable(
                name: "VendorOrders");

            migrationBuilder.RenameColumn(
                name: "VendorOrderId",
                table: "OrderItems",
                newName: "OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItems_VendorOrderId",
                table: "OrderItems",
                newName: "IX_OrderItems_OrderId");

            migrationBuilder.AddColumn<int>(
                name: "WorkshopId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OrderStatusHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    NewStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OldStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderStatusHistory_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_WorkshopId",
                table: "Orders",
                column: "WorkshopId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistory_OrderId",
                table: "OrderStatusHistory",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Temp_Orders_OrderId",
                table: "OrderItems",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Workshops_WorkshopId",
                table: "Orders",
                column: "WorkshopId",
                principalTable: "Workshops",
                principalColumn: "Id");
        }
    }
}
