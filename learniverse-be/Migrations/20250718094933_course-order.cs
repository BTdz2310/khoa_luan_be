using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace learniverse_be.Migrations
{
    /// <inheritdoc />
    public partial class courseorder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Order",
                table: "Sections",
                type: "numeric(18,6)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Sections",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Sections");

            migrationBuilder.AlterColumn<int>(
                name: "Order",
                table: "Sections",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,6)");
        }
    }
}
