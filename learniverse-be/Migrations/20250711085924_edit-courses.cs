using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace learniverse_be.Migrations
{
    /// <inheritdoc />
    public partial class editcourses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
    @"ALTER TABLE ""Courses"" 
      ALTER COLUMN ""Level"" 
      TYPE integer 
      USING ""Level""::integer;");

      migrationBuilder.Sql(
    @"ALTER TABLE ""Courses"" 
      ALTER COLUMN ""Language"" 
      TYPE integer 
      USING ""Language""::integer;");

            migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "Courses",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image",
                table: "Courses");

            migrationBuilder.AlterColumn<string>(
                name: "Level",
                table: "Courses",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Language",
                table: "Courses",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldMaxLength: 100);
        }
    }
}
