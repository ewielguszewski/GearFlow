using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GearFlow.Modules.Rentals.Infrastructure.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialRentals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "rentals");

            migrationBuilder.CreateTable(
                name: "Rentals",
                schema: "rentals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReservationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PickedUpAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReturnedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    late_fee_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    late_fee_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    rental_period_to = table.Column<DateTime>(type: "date", nullable: false),
                    rental_period_from = table.Column<DateTime>(type: "date", nullable: false),
                    total_price_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_price_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rentals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RentalLines",
                schema: "rentals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RentalId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReservationLineId = table.Column<Guid>(type: "uuid", nullable: false),
                    PickupCondition = table.Column<string>(type: "text", nullable: false),
                    PickupConditionNote = table.Column<string>(type: "text", nullable: true),
                    PickupConditionRecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReturnCondition = table.Column<string>(type: "text", nullable: true),
                    ReturnConditionNote = table.Column<string>(type: "text", nullable: true),
                    ReturnConditionRecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    damage_fee_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    damage_fee_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Item_Brand = table.Column<string>(type: "text", nullable: false),
                    Item_ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Item_Model = table.Column<string>(type: "text", nullable: false),
                    Item_PriceSource = table.Column<string>(type: "text", nullable: false),
                    Item_PublicNote = table.Column<string>(type: "text", nullable: true),
                    Item_Size = table.Column<string>(type: "text", nullable: true),
                    Item_VariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Item_VariantName = table.Column<string>(type: "text", nullable: true),
                    unit_price_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    unit_price_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    line_total_price_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    line_total_price_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RentalLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RentalLines_Rentals_RentalId",
                        column: x => x.RentalId,
                        principalSchema: "rentals",
                        principalTable: "Rentals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RentalLines_RentalId_ReservationLineId",
                schema: "rentals",
                table: "RentalLines",
                columns: new[] { "RentalId", "ReservationLineId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rentals_CustomerId",
                schema: "rentals",
                table: "Rentals",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Rentals_ReservationId",
                schema: "rentals",
                table: "Rentals",
                column: "ReservationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rentals_Status",
                schema: "rentals",
                table: "Rentals",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RentalLines",
                schema: "rentals");

            migrationBuilder.DropTable(
                name: "Rentals",
                schema: "rentals");
        }
    }
}
