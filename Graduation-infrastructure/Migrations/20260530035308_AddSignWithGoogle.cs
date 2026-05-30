using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation_infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSignWithGoogle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GoogleId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoogleId",
                table: "AspNetUsers");
        }
    }
}
