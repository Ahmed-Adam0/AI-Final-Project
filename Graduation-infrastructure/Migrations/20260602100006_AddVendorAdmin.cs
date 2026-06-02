using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation_infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVendorAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountStatus",
                table: "Workshops",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "AccountStatusChangedAt",
                table: "Workshops",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountStatusChangedByAdminId",
                table: "Workshops",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Workshops",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerificationDate",
                table: "Workshops",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerificationNotes",
                table: "Workshops",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VerificationStatus",
                table: "Workshops",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "VerifiedByAdminId",
                table: "Workshops",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "VendorAccountStatusHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkshopId = table.Column<int>(type: "int", nullable: false),
                    OldStatus = table.Column<int>(type: "int", nullable: false),
                    NewStatus = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PerformedByAdminId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorAccountStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendorAccountStatusHistory_AspNetUsers_PerformedByAdminId",
                        column: x => x.PerformedByAdminId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VendorAccountStatusHistory_Workshops_WorkshopId",
                        column: x => x.WorkshopId,
                        principalTable: "Workshops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VendorVerificationHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkshopId = table.Column<int>(type: "int", nullable: false),
                    OldStatus = table.Column<int>(type: "int", nullable: false),
                    NewStatus = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PerformedByAdminId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorVerificationHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendorVerificationHistory_AspNetUsers_PerformedByAdminId",
                        column: x => x.PerformedByAdminId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VendorVerificationHistory_Workshops_WorkshopId",
                        column: x => x.WorkshopId,
                        principalTable: "Workshops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Workshops_AccountStatusChangedByAdminId",
                table: "Workshops",
                column: "AccountStatusChangedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Workshops_VerifiedByAdminId",
                table: "Workshops",
                column: "VerifiedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorAccountStatusHistory_PerformedByAdminId",
                table: "VendorAccountStatusHistory",
                column: "PerformedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorAccountStatusHistory_WorkshopId",
                table: "VendorAccountStatusHistory",
                column: "WorkshopId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorVerificationHistory_PerformedByAdminId",
                table: "VendorVerificationHistory",
                column: "PerformedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorVerificationHistory_WorkshopId",
                table: "VendorVerificationHistory",
                column: "WorkshopId");

            migrationBuilder.AddForeignKey(
                name: "FK_Workshops_AspNetUsers_AccountStatusChangedByAdminId",
                table: "Workshops",
                column: "AccountStatusChangedByAdminId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Workshops_AspNetUsers_VerifiedByAdminId",
                table: "Workshops",
                column: "VerifiedByAdminId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workshops_AspNetUsers_AccountStatusChangedByAdminId",
                table: "Workshops");

            migrationBuilder.DropForeignKey(
                name: "FK_Workshops_AspNetUsers_VerifiedByAdminId",
                table: "Workshops");

            migrationBuilder.DropTable(
                name: "VendorAccountStatusHistory");

            migrationBuilder.DropTable(
                name: "VendorVerificationHistory");

            migrationBuilder.DropIndex(
                name: "IX_Workshops_AccountStatusChangedByAdminId",
                table: "Workshops");

            migrationBuilder.DropIndex(
                name: "IX_Workshops_VerifiedByAdminId",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "AccountStatus",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "AccountStatusChangedAt",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "AccountStatusChangedByAdminId",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "VerificationDate",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "VerificationNotes",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "VerificationStatus",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "VerifiedByAdminId",
                table: "Workshops");
        }
    }
}
