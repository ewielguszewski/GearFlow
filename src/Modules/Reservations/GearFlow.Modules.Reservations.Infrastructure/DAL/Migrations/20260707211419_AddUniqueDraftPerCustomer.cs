using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GearFlow.Modules.Reservations.Infrastructure.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueDraftPerCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Reservations_CustomerId",
                schema: "reservations",
                table: "Reservations",
                column: "CustomerId",
                unique: true,
                filter: "\"Status\" = 'Draft'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reservations_CustomerId",
                schema: "reservations",
                table: "Reservations");
        }
    }
}
