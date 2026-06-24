using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation_infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleIdIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "GoogleId",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql(@"
                WITH DuplicateUsers AS (
                    SELECT Id, GoogleId,
                           ROW_NUMBER() OVER(PARTITION BY GoogleId ORDER BY Id) as RowNum
                    FROM AspNetUsers
                    WHERE GoogleId IS NOT NULL
                )
                UPDATE AspNetUsers
                SET GoogleId = NULL
                WHERE Id IN (SELECT Id FROM DuplicateUsers WHERE RowNum > 1);
            ");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_GoogleId",
                table: "AspNetUsers",
                column: "GoogleId",
                unique: true,
                filter: "[GoogleId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_GoogleId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "GoogleId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
