using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace orders.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderItemsColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductImageUrl",
                table: "OrderItems");

            migrationBuilder.RenameColumn(
                name: "Subtotal",
                table: "OrderItems",
                newName: "UnitPrice");

            migrationBuilder.RenameColumn(
                name: "RentalDate",
                table: "OrderItems",
                newName: "ReservationDate");

            migrationBuilder.RenameColumn(
                name: "ProductThemeColor",
                table: "OrderItems",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "ProductName",
                table: "OrderItems",
                newName: "Colour");

            migrationBuilder.RenameColumn(
                name: "PricePerDay",
                table: "OrderItems",
                newName: "Total");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "OrderItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "OrderItems");

            migrationBuilder.RenameColumn(
                name: "UnitPrice",
                table: "OrderItems",
                newName: "Subtotal");

            migrationBuilder.RenameColumn(
                name: "Total",
                table: "OrderItems",
                newName: "PricePerDay");

            migrationBuilder.RenameColumn(
                name: "ReservationDate",
                table: "OrderItems",
                newName: "RentalDate");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "OrderItems",
                newName: "ProductThemeColor");

            migrationBuilder.RenameColumn(
                name: "Colour",
                table: "OrderItems",
                newName: "ProductName");

            migrationBuilder.AddColumn<string>(
                name: "ProductImageUrl",
                table: "OrderItems",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
