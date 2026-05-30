using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation_infrastructure.Migrations
{
    public partial class editOrderID : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // remove the wrong FK that used Id as FK to Orders
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Orders_Id",
                table: "OrderItems");

            // Create a temporary table with the desired schema (Id as IDENTITY + OrderId FK)
            migrationBuilder.CreateTable(
                name: "OrderItems_Temp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems_Temp", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Temp_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Temp_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_Temp_OrderId",
                table: "OrderItems_Temp",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_Temp_ProductId",
                table: "OrderItems_Temp",
                column: "ProductId");

            // Copy existing data:
            // NOTE: previous schema used OrderItems.Id as the FK to Orders.Id;
            // preserve that relationship by mapping old Id -> OrderId in the new table.
            migrationBuilder.Sql(@"
                INSERT INTO OrderItems_Temp (OrderId, ProductId, Quantity, UnitPrice, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsActive)
                SELECT Id AS OrderId, ProductId, Quantity, UnitPrice, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsActive
                FROM OrderItems
            ");

            // Drop old table (removes old PK/FKs)
            migrationBuilder.DropTable(name: "OrderItems");

            // Rename temp table to original name
            migrationBuilder.RenameTable(name: "OrderItems_Temp", newName: "OrderItems");

            // Recreate indexes/constraints names to expected defaults
            migrationBuilder.RenameIndex(
                name: "IX_OrderItems_Temp_OrderId",
                table: "OrderItems",
                newName: "IX_OrderItems_OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItems_Temp_ProductId",
                table: "OrderItems",
                newName: "IX_OrderItems_ProductId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Create old-style table without identity and with FK on Id -> Orders.Id
            migrationBuilder.CreateTable(
                name: "OrderItems_Old",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems_Old", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Old_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    // FK to Orders on Id will be added after data copy
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_Old_ProductId",
                table: "OrderItems_Old",
                column: "ProductId");

            // Copy data back: map OrderId -> Id (best-effort)
            migrationBuilder.Sql(@"
                INSERT INTO OrderItems_Old (Id, ProductId, Quantity, UnitPrice, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsActive)
                SELECT OrderId AS Id, ProductId, Quantity, UnitPrice, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsActive
                FROM OrderItems
            ");

            // Drop current OrderItems table
            migrationBuilder.DropTable(name: "OrderItems");

            // Rename old back to OrderItems
            migrationBuilder.RenameTable(name: "OrderItems_Old", newName: "OrderItems");

            // Recreate FK that originally tied OrderItems.Id to Orders.Id
            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Orders_Id",
                table: "OrderItems",
                column: "Id",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // Product FK
            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Products_ProductId",
                table: "OrderItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
