using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniDpp.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductVersionHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Products",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Products",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            // Existing products become version 1, stamped with the migration time (UTC).
            migrationBuilder.Sql("UPDATE \"Products\" SET \"UpdatedAt\" = strftime('%Y-%m-%d %H:%M:%f', 'now');");

            migrationBuilder.CreateTable(
                name: "ProductVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductGtin = table.Column<string>(type: "TEXT", maxLength: 14, nullable: false),
                    VersionNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    ModelName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    BatchNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ProductionPlace = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CottonPercentage = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    ElastanePercentage = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    CarbonKgCO2e = table.Column<decimal>(type: "TEXT", precision: 12, scale: 4, nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductVersions_Products_ProductGtin",
                        column: x => x.ProductGtin,
                        principalTable: "Products",
                        principalColumn: "Gtin",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductVersions_ProductGtin_VersionNumber",
                table: "ProductVersions",
                columns: new[] { "ProductGtin", "VersionNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductVersions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Products");
        }
    }
}
