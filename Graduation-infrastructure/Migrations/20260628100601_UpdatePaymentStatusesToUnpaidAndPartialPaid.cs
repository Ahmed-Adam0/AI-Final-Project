using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation_infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePaymentStatusesToUnpaidAndPartialPaid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Orders SET PaymentStatus = 'Unpaid' WHERE PaymentStatus = 'Pending'");
            migrationBuilder.Sql("UPDATE PaymentTransactions SET Status = 'Unpaid' WHERE Status = 'Pending'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Orders SET PaymentStatus = 'Pending' WHERE PaymentStatus = 'Unpaid'");
            migrationBuilder.Sql("UPDATE PaymentTransactions SET Status = 'Pending' WHERE Status = 'Unpaid'");
        }
    }
}
