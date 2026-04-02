using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace inventory.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingItemComponentSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BookingItemComponents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookingItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SetItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    DepositValuePerUnit = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingItemComponents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingItemComponents_BookingItems_BookingItemId",
                        column: x => x.BookingItemId,
                        principalTable: "BookingItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingItemComponents_SetItems_SetItemId",
                        column: x => x.SetItemId,
                        principalTable: "SetItems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookingItemComponents_BookingItemId",
                table: "BookingItemComponents",
                column: "BookingItemId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingItemComponents_SetItemId",
                table: "BookingItemComponents",
                column: "SetItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingItemComponents");
        }
    }
}
