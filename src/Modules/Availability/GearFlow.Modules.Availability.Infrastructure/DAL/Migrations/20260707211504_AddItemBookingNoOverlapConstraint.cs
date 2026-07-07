using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GearFlow.Modules.Availability.Infrastructure.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddItemBookingNoOverlapConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE EXTENSION IF NOT EXISTS btree_gist;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE availability."Bookings"
                ADD CONSTRAINT "EX_Bookings_ItemId_TimePeriod_NoOverlap"
                EXCLUDE USING gist (
                    "ItemId" WITH =,
                    daterange("time_period_from", "time_period_to", '[]') WITH &&
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE availability."Bookings"
                DROP CONSTRAINT IF EXISTS "EX_Bookings_ItemId_TimePeriod_NoOverlap";
                """);
        }
    }
}
