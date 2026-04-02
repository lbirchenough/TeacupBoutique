using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace inventory.Migrations
{
    /// <inheritdoc />
    public partial class AddSetItemIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "SetItems",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.UpdateData(
                table: "SetItems",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333301"),
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "SetItems",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333302"),
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "SetItems",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333303"),
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "SetItems",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333304"),
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "SetItems",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333305"),
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "SetItems",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333306"),
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "SetItems",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333307"),
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "SetItems",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333308"),
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "SetItems",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333309"),
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "SetItems",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333310"),
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "SetItems",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333311"),
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "SetItems",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333312"),
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "SetItems",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333313"),
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "SetItems",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333314"),
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "SetItems",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333315"),
                column: "IsActive",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "SetItems");
        }
    }
}
