using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace inventory.Migrations
{
    /// <inheritdoc />
    public partial class SeedInventoryItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "InventoryItems",
                columns: new[] { "Id", "Condition", "ConditionNotes", "CreatedAt", "MaintenanceHistory", "ProductId", "SerialNumber", "Status" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-2222-2222-222222222201"), 0, null, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), null, new Guid("11111111-1111-1111-1111-111111111101"), "SN-001", 0 },
                    { new Guid("22222222-2222-2222-2222-222222222202"), 0, null, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), null, new Guid("11111111-1111-1111-1111-111111111102"), "SN-002", 0 },
                    { new Guid("22222222-2222-2222-2222-222222222203"), 0, null, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), null, new Guid("11111111-1111-1111-1111-111111111103"), "SN-003", 0 },
                    { new Guid("22222222-2222-2222-2222-222222222204"), 0, null, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), null, new Guid("11111111-1111-1111-1111-111111111104"), "SN-004", 0 },
                    { new Guid("22222222-2222-2222-2222-222222222205"), 0, null, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), null, new Guid("11111111-1111-1111-1111-111111111105"), "SN-005", 0 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "InventoryItems",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222201"));

            migrationBuilder.DeleteData(
                table: "InventoryItems",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222202"));

            migrationBuilder.DeleteData(
                table: "InventoryItems",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222203"));

            migrationBuilder.DeleteData(
                table: "InventoryItems",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222204"));

            migrationBuilder.DeleteData(
                table: "InventoryItems",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222205"));
        }
    }
}
