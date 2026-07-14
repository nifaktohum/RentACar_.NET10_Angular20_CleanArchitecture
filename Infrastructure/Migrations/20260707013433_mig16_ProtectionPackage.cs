using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class mig16_ProtectionPackage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "protection");

            migrationBuilder.CreateTable(
                name: "ProtectionBenefits",
                schema: "protection",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtectionBenefits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProtectionPackages",
                schema: "protection",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsRecommended = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    StarRating = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    ProtectionLevel = table.Column<int>(type: "integer", nullable: false),
                    DeductibleType = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtectionPackages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProtectionPackageBenefits",
                schema: "protection",
                columns: table => new
                {
                    BenefitId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackageId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtectionPackageBenefits", x => new { x.BenefitId, x.PackageId });
                    table.ForeignKey(
                        name: "FK_ProtectionPackageBenefits_ProtectionBenefits_BenefitId",
                        column: x => x.BenefitId,
                        principalSchema: "protection",
                        principalTable: "ProtectionBenefits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProtectionPackageBenefits_ProtectionPackages_PackageId",
                        column: x => x.PackageId,
                        principalSchema: "protection",
                        principalTable: "ProtectionPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProtectionPricings",
                schema: "protection",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProtectionPackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    DailyPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    DeductibleAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ValidityStart = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "TIMEZONE('UTC', NOW())"),
                    ValidityEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtectionPricings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProtectionPricings_ProtectionPackages_ProtectionPackageId",
                        column: x => x.ProtectionPackageId,
                        principalSchema: "protection",
                        principalTable: "ProtectionPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProtectionBenefits_Category",
                schema: "protection",
                table: "ProtectionBenefits",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_ProtectionBenefits_DisplayOrder",
                schema: "protection",
                table: "ProtectionBenefits",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_ProtectionBenefits_Name",
                schema: "protection",
                table: "ProtectionBenefits",
                column: "Name",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_ProtectionPackageBenefits_PackageId",
                schema: "protection",
                table: "ProtectionPackageBenefits",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_ProtectionPackages_DeductibleType",
                schema: "protection",
                table: "ProtectionPackages",
                column: "DeductibleType");

            migrationBuilder.CreateIndex(
                name: "IX_ProtectionPackages_DisplayOrder",
                schema: "protection",
                table: "ProtectionPackages",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_ProtectionPackages_IsRecommended",
                schema: "protection",
                table: "ProtectionPackages",
                column: "IsRecommended");

            migrationBuilder.CreateIndex(
                name: "IX_ProtectionPackages_Name",
                schema: "protection",
                table: "ProtectionPackages",
                column: "Name",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_ProtectionPackages_ProtectionLevel",
                schema: "protection",
                table: "ProtectionPackages",
                column: "ProtectionLevel");

            migrationBuilder.CreateIndex(
                name: "IX_ProtectionPricings_ProtectionPackageId",
                schema: "protection",
                table: "ProtectionPricings",
                column: "ProtectionPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_ProtectionPricings_ProtectionPackageId_IsDefault",
                schema: "protection",
                table: "ProtectionPricings",
                columns: new[] { "ProtectionPackageId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_ProtectionPricings_ValidityEnd",
                schema: "protection",
                table: "ProtectionPricings",
                column: "ValidityEnd");

            migrationBuilder.CreateIndex(
                name: "IX_ProtectionPricings_ValidityStart",
                schema: "protection",
                table: "ProtectionPricings",
                column: "ValidityStart");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProtectionPackageBenefits",
                schema: "protection");

            migrationBuilder.DropTable(
                name: "ProtectionPricings",
                schema: "protection");

            migrationBuilder.DropTable(
                name: "ProtectionBenefits",
                schema: "protection");

            migrationBuilder.DropTable(
                name: "ProtectionPackages",
                schema: "protection");
        }
    }
}
