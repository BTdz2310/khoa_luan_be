using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace learniverse_be.Migrations
{
    /// <inheritdoc />
    public partial class video : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Livestream",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Livestream", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Videos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LectureId = table.Column<Guid>(type: "uuid", nullable: true),
                    LivestreamId = table.Column<Guid>(type: "uuid", nullable: true),
                    OriginalFileName = table.Column<string>(type: "text", nullable: false),
                    OriginalFileKey = table.Column<string>(type: "text", nullable: false),
                    OriginalMimeType = table.Column<string>(type: "text", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    HlsMasterPlaylistUrl = table.Column<string>(type: "text", nullable: true),
                    HlsDirectoryKey = table.Column<string>(type: "text", nullable: true),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: true),
                    Width = table.Column<int>(type: "integer", nullable: true),
                    Height = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Videos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Videos_Lectures_LectureId",
                        column: x => x.LectureId,
                        principalTable: "Lectures",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Videos_Livestream_LivestreamId",
                        column: x => x.LivestreamId,
                        principalTable: "Livestream",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Videos_LectureId",
                table: "Videos",
                column: "LectureId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Videos_LivestreamId",
                table: "Videos",
                column: "LivestreamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Videos");

            migrationBuilder.DropTable(
                name: "Livestream");
        }
    }
}
