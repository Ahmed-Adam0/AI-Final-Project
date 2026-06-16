using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation_infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Drop existing FK
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            // 2. Create SubCategories and ProductTypes tables
            migrationBuilder.CreateTable(
                name: "SubCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SubCategoryId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductTypes_SubCategories_SubCategoryId",
                        column: x => x.SubCategoryId,
                        principalTable: "SubCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductTypes_SubCategoryId",
                table: "ProductTypes",
                column: "SubCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SubCategories_CategoryId",
                table: "SubCategories",
                column: "CategoryId");

            // 3. Rename Column CategoryId to ProductTypeId on Products
            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Products",
                newName: "ProductTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                newName: "IX_Products_ProductTypeId");

            // 4. Custom Data Migration SQL
            migrationBuilder.Sql(@"
                -- Insert Top-Level Categories
                INSERT INTO Categories (NameAr, NameEn, ImageUrl, CreatedAt, IsActive)
                VALUES 
                (N'الأثاث', 'Furniture', '', GETUTCDATE(), 1),
                (N'الإضاءة', 'Lighting', '', GETUTCDATE(), 1),
                (N'الديكور', 'Decor', '', GETUTCDATE(), 1),
                (N'الستائر', 'Curtains', '', GETUTCDATE(), 1),
                (N'السجاد', 'Carpets', '', GETUTCDATE(), 1);

                DECLARE @FurnitureId INT, @LightingId INT, @DecorId INT, @CurtainsId INT, @CarpetsId INT;
                SELECT @FurnitureId = Id FROM Categories WHERE NameEn = 'Furniture';
                SELECT @LightingId = Id FROM Categories WHERE NameEn = 'Lighting';
                SELECT @DecorId = Id FROM Categories WHERE NameEn = 'Decor';
                SELECT @CurtainsId = Id FROM Categories WHERE NameEn = 'Curtains';
                SELECT @CarpetsId = Id FROM Categories WHERE NameEn = 'Carpets';

                -- Insert SubCategories
                INSERT INTO SubCategories (NameAr, NameEn, CategoryId, CreatedAt, IsActive)
                VALUES
                -- Furniture
                (N'غرفة المعيشة', 'Living Room', @FurnitureId, GETUTCDATE(), 1),
                (N'غرفة النوم', 'Bedroom', @FurnitureId, GETUTCDATE(), 1),
                (N'غرفة الطعام', 'Dining Room', @FurnitureId, GETUTCDATE(), 1),
                (N'المكتب', 'Office', @FurnitureId, GETUTCDATE(), 1),
                (N'الخارجية', 'Outdoor', @FurnitureId, GETUTCDATE(), 1),
                -- Lighting
                (N'إضاءة داخلية', 'Indoor Lighting', @LightingId, GETUTCDATE(), 1),
                (N'إضاءة خارجية', 'Outdoor Lighting', @LightingId, GETUTCDATE(), 1),
                (N'إضاءة ذكية', 'Smart Lighting', @LightingId, GETUTCDATE(), 1),
                -- Decor
                (N'ديكور حائط', 'Wall Decor', @DecorId, GETUTCDATE(), 1),
                (N'إكسسوارات منزلية', 'Home Accessories', @DecorId, GETUTCDATE(), 1),
                -- Curtains & Carpets
                (N'الستائر', 'Curtains', @CurtainsId, GETUTCDATE(), 1),
                (N'السجاد', 'Carpets', @CarpetsId, GETUTCDATE(), 1);

                DECLARE @LivingRoomId INT, @BedroomSubId INT, @DiningRoomId INT, @OfficeSubId INT, @OutdoorSubId INT;
                SELECT @LivingRoomId = Id FROM SubCategories WHERE NameEn = 'Living Room';
                SELECT @BedroomSubId = Id FROM SubCategories WHERE NameEn = 'Bedroom';
                SELECT @DiningRoomId = Id FROM SubCategories WHERE NameEn = 'Dining Room';
                SELECT @OfficeSubId = Id FROM SubCategories WHERE NameEn = 'Office';
                SELECT @OutdoorSubId = Id FROM SubCategories WHERE NameEn = 'Outdoor';

                DECLARE @IndoorLightingId INT, @OutdoorLightingId INT, @SmartLightingId INT;
                SELECT @IndoorLightingId = Id FROM SubCategories WHERE NameEn = 'Indoor Lighting';
                SELECT @OutdoorLightingId = Id FROM SubCategories WHERE NameEn = 'Outdoor Lighting';
                SELECT @SmartLightingId = Id FROM SubCategories WHERE NameEn = 'Smart Lighting';

                DECLARE @WallDecorId INT, @HomeAccessoriesId INT;
                SELECT @WallDecorId = Id FROM SubCategories WHERE NameEn = 'Wall Decor';
                SELECT @HomeAccessoriesId = Id FROM SubCategories WHERE NameEn = 'Home Accessories';

                DECLARE @CurtainsSubId INT, @CarpetsSubId INT;
                SELECT @CurtainsSubId = Id FROM SubCategories WHERE NameEn = 'Curtains';
                SELECT @CarpetsSubId = Id FROM SubCategories WHERE NameEn = 'Carpets';

                -- Insert ProductTypes
                INSERT INTO ProductTypes (NameAr, NameEn, SubCategoryId, CreatedAt, IsActive)
                VALUES
                -- Living Room
                (N'أريكة', 'Sofa', @LivingRoomId, GETUTCDATE(), 1),
                (N'أريكة زاوية', 'Corner Sofa', @LivingRoomId, GETUTCDATE(), 1),
                (N'طاولة قهوة', 'Coffee Table', @LivingRoomId, GETUTCDATE(), 1),
                (N'طاولة تلفزيون', 'TV Unit', @LivingRoomId, GETUTCDATE(), 1),
                (N'كرسي ذراعين', 'Arm Chair', @LivingRoomId, GETUTCDATE(), 1),
                -- Bedroom
                (N'سرير', 'Bed', @BedroomSubId, GETUTCDATE(), 1),
                (N'خزانة ملابس', 'Wardrobe', @BedroomSubId, GETUTCDATE(), 1),
                (N'طاولة سرير جانبية', 'Nightstand', @BedroomSubId, GETUTCDATE(), 1),
                (N'تسريحة', 'Dressing Table', @BedroomSubId, GETUTCDATE(), 1),
                -- Dining Room
                (N'طاولة طعام', 'Dining Table', @DiningRoomId, GETUTCDATE(), 1),
                (N'كرسي طعام', 'Dining Chair', @DiningRoomId, GETUTCDATE(), 1),
                (N'بوفيه', 'Buffet', @DiningRoomId, GETUTCDATE(), 1),
                -- Office
                (N'مكتب', 'Office Desk', @OfficeSubId, GETUTCDATE(), 1),
                (N'كرسي مكتب', 'Office Chair', @OfficeSubId, GETUTCDATE(), 1),
                (N'مكتبة كتب', 'Library', @OfficeSubId, GETUTCDATE(), 1),
                -- Outdoor
                (N'كرسي حديقة', 'Garden Chair', @OutdoorSubId, GETUTCDATE(), 1),
                (N'طاولة حديقة', 'Garden Table', @OutdoorSubId, GETUTCDATE(), 1),
                (N'أرجوحة', 'Swing', @OutdoorSubId, GETUTCDATE(), 1),
                -- Indoor Lighting
                (N'نجفة', 'Chandelier', @IndoorLightingId, GETUTCDATE(), 1),
                (N'أباجورة طاولة', 'Table Lamp', @IndoorLightingId, GETUTCDATE(), 1),
                -- Outdoor Lighting
                (N'إضاءة حديقة', 'Garden Light', @OutdoorLightingId, GETUTCDATE(), 1),
                -- Smart Lighting
                (N'لمبة ذكية', 'Smart Bulb', @SmartLightingId, GETUTCDATE(), 1),
                -- Wall Decor
                (N'لوحات فنية', 'Paintings', @WallDecorId, GETUTCDATE(), 1),
                (N'مرايا', 'Mirrors', @WallDecorId, GETUTCDATE(), 1),
                -- Home Accessories
                (N'فازة', 'Vase', @HomeAccessoriesId, GETUTCDATE(), 1),
                -- Curtains
                (N'ستارة كلاسيكية', 'Classic Curtain', @CurtainsSubId, GETUTCDATE(), 1),
                (N'ستارة حديثة', 'Modern Curtain', @CurtainsSubId, GETUTCDATE(), 1),
                (N'ستارة بلاك أوت', 'Blackout Curtain', @CurtainsSubId, GETUTCDATE(), 1),
                -- Carpets
                (N'سجاد حديث', 'Modern Carpet', @CarpetsSubId, GETUTCDATE(), 1),
                (N'سجاد كلاسيكي', 'Classic Carpet', @CarpetsSubId, GETUTCDATE(), 1),
                (N'سجاد أطفال', 'Kids Carpet', @CarpetsSubId, GETUTCDATE(), 1);

                -- Mapping Existing Products
                DECLARE @SofaId INT, @BedId INT, @DeskId INT;
                SELECT @SofaId = Id FROM ProductTypes WHERE NameEn = 'Sofa';
                SELECT @BedId = Id FROM ProductTypes WHERE NameEn = 'Bed';
                SELECT @DeskId = Id FROM ProductTypes WHERE NameEn = 'Office Desk';

                -- Map old Category "" غرف نوم"" / ""Bedroom"" to Bed ProductType
                UPDATE Products
                SET ProductTypeId = @BedId
                WHERE ProductTypeId IN (SELECT Id FROM Categories WHERE NameEn = 'Bedroom' OR NameAr = N'غرف نوم');

                -- Map old Category ""غرف معيشة"" / ""Living Room"" to Sofa ProductType
                UPDATE Products
                SET ProductTypeId = @SofaId
                WHERE ProductTypeId IN (SELECT Id FROM Categories WHERE NameEn = 'Living Room' OR NameAr = N'غرف معيشة');

                -- Map old Category ""مكاتب"" / ""Office"" to Office Desk ProductType
                UPDATE Products
                SET ProductTypeId = @DeskId
                WHERE ProductTypeId IN (SELECT Id FROM Categories WHERE NameEn = 'Office' OR NameAr = N'مكاتب');

                -- Set default to Sofa if any remaining unmapped (should be none)
                UPDATE Products
                SET ProductTypeId = @SofaId
                WHERE ProductTypeId NOT IN (SELECT Id FROM ProductTypes);

                -- Clean up old categories that were mapped
                DELETE FROM Categories WHERE NameEn IN ('Bedroom', 'Living Room', 'Office') OR NameAr IN (N'غرف نوم', N'غرف معيشة', N'مكاتب');
            ");

            // 5. Add Foreign Key
            migrationBuilder.AddForeignKey(
                name: "FK_Products_ProductTypes_ProductTypeId",
                table: "Products",
                column: "ProductTypeId",
                principalTable: "ProductTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_ProductTypes_ProductTypeId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "ProductTypes");

            migrationBuilder.DropTable(
                name: "SubCategories");

            migrationBuilder.RenameColumn(
                name: "ProductTypeId",
                table: "Products",
                newName: "CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Products_ProductTypeId",
                table: "Products",
                newName: "IX_Products_CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
