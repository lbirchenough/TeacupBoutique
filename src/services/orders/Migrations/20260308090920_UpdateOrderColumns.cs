using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace orders.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "EventType",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "GuestCount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SpecialRequests",
                table: "Orders");

            migrationBuilder.AddColumn<DateOnly>(
                name: "ReservationDate",
                table: "Orders",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReservationDate",
                table: "Orders");

            migrationBuilder.AddColumn<DateOnly>(
                name: "EventDate",
                table: "Orders",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EventType",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GuestCount",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecialRequests",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
