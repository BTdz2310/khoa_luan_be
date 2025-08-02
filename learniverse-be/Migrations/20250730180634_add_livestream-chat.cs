using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace learniverse_be.Migrations
{
    /// <inheritdoc />
    public partial class add_livestreamchat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VideoChats_Videos_VideoId",
                table: "VideoChats");

            migrationBuilder.AlterColumn<Guid>(
                name: "VideoId",
                table: "VideoChats",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "LivestreamId",
                table: "VideoChats",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VideoChats_LivestreamId",
                table: "VideoChats",
                column: "LivestreamId");

            migrationBuilder.AddForeignKey(
                name: "FK_VideoChats_Livestreams_LivestreamId",
                table: "VideoChats",
                column: "LivestreamId",
                principalTable: "Livestreams",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VideoChats_Videos_VideoId",
                table: "VideoChats",
                column: "VideoId",
                principalTable: "Videos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VideoChats_Livestreams_LivestreamId",
                table: "VideoChats");

            migrationBuilder.DropForeignKey(
                name: "FK_VideoChats_Videos_VideoId",
                table: "VideoChats");

            migrationBuilder.DropIndex(
                name: "IX_VideoChats_LivestreamId",
                table: "VideoChats");

            migrationBuilder.DropColumn(
                name: "LivestreamId",
                table: "VideoChats");

            migrationBuilder.AlterColumn<Guid>(
                name: "VideoId",
                table: "VideoChats",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_VideoChats_Videos_VideoId",
                table: "VideoChats",
                column: "VideoId",
                principalTable: "Videos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
