using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GearFlow.Modules.Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CatalogInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "catalog");

            migrationBuilder.CreateTable(
                name: "EquipmentModels",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Brand = table.Column<string>(type: "text", nullable: false),
                    Model = table.Column<string>(type: "text", nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    base_price_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    base_price_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EquipmentVariants",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    PublicNote = table.Column<string>(type: "text", nullable: true),
                    overridden_price_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    overridden_price_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    Size = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EquipmentVariants_EquipmentModels_EquipmentModelId",
                        column: x => x.EquipmentModelId,
                        principalSchema: "catalog",
                        principalTable: "EquipmentModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EquipmentItems",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetTag = table.Column<string>(type: "text", nullable: false),
                    InternalNote = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    LastMaintenanceAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextInspectionAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EquipmentItems_EquipmentVariants_EquipmentVariantId",
                        column: x => x.EquipmentVariantId,
                        principalSchema: "catalog",
                        principalTable: "EquipmentVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentItems_EquipmentVariantId",
                schema: "catalog",
                table: "EquipmentItems",
                column: "EquipmentVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentVariants_EquipmentModelId",
                schema: "catalog",
                table: "EquipmentVariants",
                column: "EquipmentModelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EquipmentItems",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "EquipmentVariants",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "EquipmentModels",
                schema: "catalog");
        }
    }
}
