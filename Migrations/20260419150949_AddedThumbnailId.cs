using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstralDiaryApi.Migrations
{
    /// <inheritdoc />
    public partial class AddedThumbnailId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "att_file_path",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "att_file_path",
                table: "Drafts");

            migrationBuilder.RenameColumn(
                name: "att_thumbnail_path",
                table: "Entries",
                newName: "thumbnail_id");

            migrationBuilder.RenameColumn(
                name: "att_thumbnail_path",
                table: "Drafts",
                newName: "thumbnail_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "thumbnail_id",
                table: "Entries",
                newName: "att_thumbnail_path");

            migrationBuilder.RenameColumn(
                name: "thumbnail_id",
                table: "Drafts",
                newName: "att_thumbnail_path");

            migrationBuilder.AddColumn<string>(
                name: "att_file_path",
                table: "Entries",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "att_file_path",
                table: "Drafts",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
