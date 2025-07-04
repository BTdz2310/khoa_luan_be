using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace learniverse_be.Migrations
{
    /// <inheritdoc />
    public partial class migrationinstructorusercate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Instructor_Auths_AuthId",
                table: "Instructor");

            migrationBuilder.DropForeignKey(
                name: "FK_UserCategory_Categories_CategoryId",
                table: "UserCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_UserCategory_Users_UserId",
                table: "UserCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserCategory",
                table: "UserCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Instructor",
                table: "Instructor");

            migrationBuilder.RenameTable(
                name: "UserCategory",
                newName: "UserCategories");

            migrationBuilder.RenameTable(
                name: "Instructor",
                newName: "Instructors");

            migrationBuilder.RenameIndex(
                name: "IX_UserCategory_CategoryId",
                table: "UserCategories",
                newName: "IX_UserCategories_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Instructor_AuthId",
                table: "Instructors",
                newName: "IX_Instructors_AuthId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserCategories",
                table: "UserCategories",
                columns: new[] { "UserId", "CategoryId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Instructors",
                table: "Instructors",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Instructors_DisplayName",
                table: "Instructors",
                column: "DisplayName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Instructors_Auths_AuthId",
                table: "Instructors",
                column: "AuthId",
                principalTable: "Auths",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserCategories_Categories_CategoryId",
                table: "UserCategories",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserCategories_Users_UserId",
                table: "UserCategories",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Instructors_Auths_AuthId",
                table: "Instructors");

            migrationBuilder.DropForeignKey(
                name: "FK_UserCategories_Categories_CategoryId",
                table: "UserCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_UserCategories_Users_UserId",
                table: "UserCategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserCategories",
                table: "UserCategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Instructors",
                table: "Instructors");

            migrationBuilder.DropIndex(
                name: "IX_Instructors_DisplayName",
                table: "Instructors");

            migrationBuilder.RenameTable(
                name: "UserCategories",
                newName: "UserCategory");

            migrationBuilder.RenameTable(
                name: "Instructors",
                newName: "Instructor");

            migrationBuilder.RenameIndex(
                name: "IX_UserCategories_CategoryId",
                table: "UserCategory",
                newName: "IX_UserCategory_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Instructors_AuthId",
                table: "Instructor",
                newName: "IX_Instructor_AuthId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserCategory",
                table: "UserCategory",
                columns: new[] { "UserId", "CategoryId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Instructor",
                table: "Instructor",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Instructor_Auths_AuthId",
                table: "Instructor",
                column: "AuthId",
                principalTable: "Auths",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserCategory_Categories_CategoryId",
                table: "UserCategory",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserCategory_Users_UserId",
                table: "UserCategory",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
