using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace orders.Migrations
{
    /// <inheritdoc />
    public partial class AddDepositTotal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DepositTotal",
                table: "Orders",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepositTotal",
                table: "Orders");
        }
    }
}
