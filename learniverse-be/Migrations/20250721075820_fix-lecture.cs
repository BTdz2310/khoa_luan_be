using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace learniverse_be.Migrations
{
    /// <inheritdoc />
    public partial class fixlecture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Order",
                table: "Lectures",
                type: "numeric(18,6)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Lectures",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Lectures",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Lectures");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Lectures");

            migrationBuilder.AlterColumn<int>(
                name: "Order",
                table: "Lectures",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,6)");
        }
    }
}
