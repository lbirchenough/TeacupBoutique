using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace inventory.Migrations
{
    /// <inheritdoc />
    public partial class MoveCleanedAtToProductSet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CleanedAt",
                table: "Bookings");

            migrationBuilder.AddColumn<DateTime>(
                name: "CleanedAt",
                table: "ProductSets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ProductSets",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222201"),
                column: "CleanedAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "ProductSets",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222202"),
                column: "CleanedAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "ProductSets",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222203"),
                column: "CleanedAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "ProductSets",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222204"),
                column: "CleanedAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "ProductSets",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222205"),
                column: "CleanedAt",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CleanedAt",
                table: "ProductSets");

            migrationBuilder.AddColumn<DateTime>(
                name: "CleanedAt",
                table: "Bookings",
                type: "datetime2",
                nullable: true);
        }
    }
}
