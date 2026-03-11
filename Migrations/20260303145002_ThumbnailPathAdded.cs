using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstralDiaryApi.Migrations
{
    /// <inheritdoc />
    public partial class ThumbnailPathAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "object_storage_key",
                table: "Attachments",
                newName: "thumbnail_path");

            migrationBuilder.AddColumn<string>(
                name: "file_path",
                table: "Attachments",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "file_path",
                table: "Attachments");

            migrationBuilder.RenameColumn(
                name: "thumbnail_path",
                table: "Attachments",
                newName: "object_storage_key");
        }
    }
}
