using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation_infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVendorOrderStatusProcessingToInProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE VendorOrders SET Status = 'InProgress' WHERE Status = 'Processing'");
            migrationBuilder.Sql("UPDATE VendorOrderStatusHistories SET NewStatus = 'InProgress' WHERE NewStatus = 'Processing'");
            migrationBuilder.Sql("UPDATE VendorOrderStatusHistories SET OldStatus = 'InProgress' WHERE OldStatus = 'Processing'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE VendorOrders SET Status = 'Processing' WHERE Status = 'InProgress'");
            migrationBuilder.Sql("UPDATE VendorOrderStatusHistories SET NewStatus = 'Processing' WHERE NewStatus = 'InProgress'");
            migrationBuilder.Sql("UPDATE VendorOrderStatusHistories SET OldStatus = 'Processing' WHERE OldStatus = 'InProgress'");
        }
    }
}
