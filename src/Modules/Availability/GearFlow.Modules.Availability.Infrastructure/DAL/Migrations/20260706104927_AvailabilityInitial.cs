using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GearFlow.Modules.Availability.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AvailabilityInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "availability");

            migrationBuilder.CreateTable(
                name: "Bookings",
                schema: "availability",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    VariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    time_period_to = table.Column<DateTime>(type: "date", nullable: false),
                    time_period_from = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bookings",
                schema: "availability");
        }
    }
}
