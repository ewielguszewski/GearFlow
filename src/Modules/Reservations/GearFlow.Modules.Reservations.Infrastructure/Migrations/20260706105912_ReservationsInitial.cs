using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GearFlow.Modules.Reservations.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReservationsInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "reservations");

            migrationBuilder.CreateTable(
                name: "Reservations",
                schema: "reservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CancReason = table.Column<string>(type: "text", nullable: true),
                    SelectedPaymentMethod = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TtlExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    paid_amount_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    paid_amount_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    reserved_period_to = table.Column<DateTime>(type: "date", nullable: false),
                    reserved_period_from = table.Column<DateTime>(type: "date", nullable: false),
                    total_price_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_price_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReservationLines",
                schema: "reservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReservationId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_ReservationLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReservationLines_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalSchema: "reservations",
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReservationLines_ReservationId",
                schema: "reservations",
                table: "ReservationLines",
                column: "ReservationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReservationLines",
                schema: "reservations");

            migrationBuilder.DropTable(
                name: "Reservations",
                schema: "reservations");
        }
    }
}
