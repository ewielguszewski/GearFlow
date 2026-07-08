using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GearFlow.Modules.Reservations.Infrastructure.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationStartDateConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Reservations_ReservedPeriod_Start_NotBeforeCreatedAtUtcDate",
                schema: "reservations",
                table: "Reservations",
                sql: "\"reserved_period_from\" >= ((\"CreatedAt\" AT TIME ZONE 'UTC')::date)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Reservations_ReservedPeriod_Start_NotBeforeCreatedAtUtcDate",
                schema: "reservations",
                table: "Reservations");
        }
    }
}
