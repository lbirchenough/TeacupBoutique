using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace orders.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderCompletionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompletionNotes",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DepositAmountKept",
                table: "Orders",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnPhotoUrls",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletionNotes",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DepositAmountKept",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ReturnPhotoUrls",
                table: "Orders");
        }
    }
}
