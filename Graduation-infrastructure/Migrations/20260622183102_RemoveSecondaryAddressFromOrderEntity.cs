using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation_infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSecondaryAddressFromOrderEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SecondaryAddress",
                table: "Orders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SecondaryAddress",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
