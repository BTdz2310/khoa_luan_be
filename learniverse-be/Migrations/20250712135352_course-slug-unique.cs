using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace learniverse_be.Migrations
{
    /// <inheritdoc />
    public partial class courseslugunique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Courses_Slug",
                table: "Courses",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Courses_Slug",
                table: "Courses");
        }
    }
}
