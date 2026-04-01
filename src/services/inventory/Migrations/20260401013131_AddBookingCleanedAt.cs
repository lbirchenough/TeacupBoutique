using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace inventory.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingCleanedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CleanedAt",
                table: "Bookings",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CleanedAt",
                table: "Bookings");
        }
    }
}
