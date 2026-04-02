using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace inventory.Migrations
{
    /// <inheritdoc />
    public partial class InventoryReworkAndReturnFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingItems_InventoryItems_InventoryItemId",
                table: "BookingItems");

            migrationBuilder.DropTable(
                name: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "ReturnCondition",
                table: "BookingItems");

            migrationBuilder.RenameColumn(
                name: "InventoryItemId",
                table: "BookingItems",
                newName: "ProductSetId");

            migrationBuilder.RenameIndex(
                name: "IX_BookingItems_InventoryItemId",
                table: "BookingItems",
                newName: "IX_BookingItems_ProductSetId");

            migrationBuilder.CreateTable(
                name: "ProductSets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductSets_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SetItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    DepositValuePerUnit = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SetItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReturnAssessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookingItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SetItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuantityGood = table.Column<int>(type: "int", nullable: false),
                    QuantityDamaged = table.Column<int>(type: "int", nullable: false),
                    QuantityMissing = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReplacedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReturnAssessments_BookingItems_BookingItemId",
                        column: x => x.BookingItemId,
                        principalTable: "BookingItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReturnAssessments_SetItems_SetItemId",
                        column: x => x.SetItemId,
                        principalTable: "SetItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpareStocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SetItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuantityAvailable = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpareStocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpareStocks_SetItems_SetItemId",
                        column: x => x.SetItemId,
                        principalTable: "SetItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ProductSets",
                columns: new[] { "Id", "CreatedAt", "Name", "ProductId", "Status" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-2222-2222-222222222201"), new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "Set A", new Guid("11111111-1111-1111-1111-111111111101"), 0 },
                    { new Guid("22222222-2222-2222-2222-222222222202"), new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "Set A", new Guid("11111111-1111-1111-1111-111111111102"), 0 },
                    { new Guid("22222222-2222-2222-2222-222222222203"), new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "Set A", new Guid("11111111-1111-1111-1111-111111111103"), 0 },
                    { new Guid("22222222-2222-2222-2222-222222222204"), new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "Set A", new Guid("11111111-1111-1111-1111-111111111104"), 0 },
                    { new Guid("22222222-2222-2222-2222-222222222205"), new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "Set A", new Guid("11111111-1111-1111-1111-111111111105"), 0 }
                });

            migrationBuilder.InsertData(
                table: "SetItems",
                columns: new[] { "Id", "DepositValuePerUnit", "Name", "ProductId", "Quantity" },
                values: new object[,]
                {
                    { new Guid("33333333-3333-3333-3333-333333333301"), 25.00m, "Teapot", new Guid("11111111-1111-1111-1111-111111111101"), 1 },
                    { new Guid("33333333-3333-3333-3333-333333333302"), 8.00m, "Teacups & Saucers", new Guid("11111111-1111-1111-1111-111111111101"), 6 },
                    { new Guid("33333333-3333-3333-3333-333333333303"), 7.00m, "Plates & Stand", new Guid("11111111-1111-1111-1111-111111111101"), 1 },
                    { new Guid("33333333-3333-3333-3333-333333333304"), 30.00m, "Teapot", new Guid("11111111-1111-1111-1111-111111111102"), 1 },
                    { new Guid("33333333-3333-3333-3333-333333333305"), 10.00m, "Teacups & Saucers", new Guid("11111111-1111-1111-1111-111111111102"), 4 },
                    { new Guid("33333333-3333-3333-3333-333333333306"), 30.00m, "Plates & Stand", new Guid("11111111-1111-1111-1111-111111111102"), 1 },
                    { new Guid("33333333-3333-3333-3333-333333333307"), 30.00m, "Teapot", new Guid("11111111-1111-1111-1111-111111111103"), 1 },
                    { new Guid("33333333-3333-3333-3333-333333333308"), 8.00m, "Teacups & Saucers", new Guid("11111111-1111-1111-1111-111111111103"), 8 },
                    { new Guid("33333333-3333-3333-3333-333333333309"), 26.00m, "Plates & Stand", new Guid("11111111-1111-1111-1111-111111111103"), 1 },
                    { new Guid("33333333-3333-3333-3333-333333333310"), 25.00m, "Teapot", new Guid("11111111-1111-1111-1111-111111111104"), 1 },
                    { new Guid("33333333-3333-3333-3333-333333333311"), 8.00m, "Teacups & Saucers", new Guid("11111111-1111-1111-1111-111111111104"), 4 },
                    { new Guid("33333333-3333-3333-3333-333333333312"), 13.00m, "Plates & Stand", new Guid("11111111-1111-1111-1111-111111111104"), 1 },
                    { new Guid("33333333-3333-3333-3333-333333333313"), 35.00m, "Teapot", new Guid("11111111-1111-1111-1111-111111111105"), 1 },
                    { new Guid("33333333-3333-3333-3333-333333333314"), 12.00m, "Teacups & Saucers", new Guid("11111111-1111-1111-1111-111111111105"), 6 },
                    { new Guid("33333333-3333-3333-3333-333333333315"), 33.00m, "Plates & Accessories", new Guid("11111111-1111-1111-1111-111111111105"), 1 }
                });

            migrationBuilder.InsertData(
                table: "SpareStocks",
                columns: new[] { "Id", "QuantityAvailable", "SetItemId" },
                values: new object[,]
                {
                    { new Guid("44444444-4444-4444-4444-444444444401"), 2, new Guid("33333333-3333-3333-3333-333333333301") },
                    { new Guid("44444444-4444-4444-4444-444444444402"), 2, new Guid("33333333-3333-3333-3333-333333333302") },
                    { new Guid("44444444-4444-4444-4444-444444444403"), 2, new Guid("33333333-3333-3333-3333-333333333303") },
                    { new Guid("44444444-4444-4444-4444-444444444404"), 2, new Guid("33333333-3333-3333-3333-333333333304") },
                    { new Guid("44444444-4444-4444-4444-444444444405"), 2, new Guid("33333333-3333-3333-3333-333333333305") },
                    { new Guid("44444444-4444-4444-4444-444444444406"), 2, new Guid("33333333-3333-3333-3333-333333333306") },
                    { new Guid("44444444-4444-4444-4444-444444444407"), 2, new Guid("33333333-3333-3333-3333-333333333307") },
                    { new Guid("44444444-4444-4444-4444-444444444408"), 2, new Guid("33333333-3333-3333-3333-333333333308") },
                    { new Guid("44444444-4444-4444-4444-444444444409"), 2, new Guid("33333333-3333-3333-3333-333333333309") },
                    { new Guid("44444444-4444-4444-4444-444444444410"), 2, new Guid("33333333-3333-3333-3333-333333333310") },
                    { new Guid("44444444-4444-4444-4444-444444444411"), 2, new Guid("33333333-3333-3333-3333-333333333311") },
                    { new Guid("44444444-4444-4444-4444-444444444412"), 2, new Guid("33333333-3333-3333-3333-333333333312") },
                    { new Guid("44444444-4444-4444-4444-444444444413"), 2, new Guid("33333333-3333-3333-3333-333333333313") },
                    { new Guid("44444444-4444-4444-4444-444444444414"), 2, new Guid("33333333-3333-3333-3333-333333333314") },
                    { new Guid("44444444-4444-4444-4444-444444444415"), 2, new Guid("33333333-3333-3333-3333-333333333315") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductSets_ProductId",
                table: "ProductSets",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnAssessments_BookingItemId",
                table: "ReturnAssessments",
                column: "BookingItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnAssessments_SetItemId",
                table: "ReturnAssessments",
                column: "SetItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SetItems_ProductId",
                table: "SetItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SpareStocks_SetItemId",
                table: "SpareStocks",
                column: "SetItemId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingItems_ProductSets_ProductSetId",
                table: "BookingItems",
                column: "ProductSetId",
                principalTable: "ProductSets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingItems_ProductSets_ProductSetId",
                table: "BookingItems");

            migrationBuilder.DropTable(
                name: "ProductSets");

            migrationBuilder.DropTable(
                name: "ReturnAssessments");

            migrationBuilder.DropTable(
                name: "SpareStocks");

            migrationBuilder.DropTable(
                name: "SetItems");

            migrationBuilder.RenameColumn(
                name: "ProductSetId",
                table: "BookingItems",
                newName: "InventoryItemId");

            migrationBuilder.RenameIndex(
                name: "IX_BookingItems_ProductSetId",
                table: "BookingItems",
                newName: "IX_BookingItems_InventoryItemId");

            migrationBuilder.AddColumn<int>(
                name: "ReturnCondition",
                table: "BookingItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InventoryItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Condition = table.Column<int>(type: "int", nullable: false),
                    ConditionNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaintenanceHistory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_ProductId",
                table: "InventoryItems",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingItems_InventoryItems_InventoryItemId",
                table: "BookingItems",
                column: "InventoryItemId",
                principalTable: "InventoryItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
