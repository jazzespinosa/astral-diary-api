using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstralDiaryApi.Migrations
{
    /// <inheritdoc />
    public partial class AddedIvAndSaltForAttachment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "salt",
                table: "Entries",
                newName: "encrypted_content");

            migrationBuilder.RenameColumn(
                name: "iv",
                table: "Entries",
                newName: "content_salt");

            migrationBuilder.RenameColumn(
                name: "encrypted_data",
                table: "Entries",
                newName: "content_iv");

            migrationBuilder.RenameColumn(
                name: "salt",
                table: "Drafts",
                newName: "encrypted_content");

            migrationBuilder.RenameColumn(
                name: "iv",
                table: "Drafts",
                newName: "content_salt");

            migrationBuilder.RenameColumn(
                name: "encrypted_data",
                table: "Drafts",
                newName: "content_iv");

            migrationBuilder.AddColumn<string>(
                name: "att_iv",
                table: "Entries",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "att_salt",
                table: "Entries",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "thumbnail_iv",
                table: "Entries",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "thumbnail_salt",
                table: "Entries",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "att_iv",
                table: "Drafts",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "att_salt",
                table: "Drafts",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "thumbnail_iv",
                table: "Drafts",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "thumbnail_salt",
                table: "Drafts",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "att_iv",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "att_salt",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "thumbnail_iv",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "thumbnail_salt",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "att_iv",
                table: "Drafts");

            migrationBuilder.DropColumn(
                name: "att_salt",
                table: "Drafts");

            migrationBuilder.DropColumn(
                name: "thumbnail_iv",
                table: "Drafts");

            migrationBuilder.DropColumn(
                name: "thumbnail_salt",
                table: "Drafts");

            migrationBuilder.RenameColumn(
                name: "encrypted_content",
                table: "Entries",
                newName: "salt");

            migrationBuilder.RenameColumn(
                name: "content_salt",
                table: "Entries",
                newName: "iv");

            migrationBuilder.RenameColumn(
                name: "content_iv",
                table: "Entries",
                newName: "encrypted_data");

            migrationBuilder.RenameColumn(
                name: "encrypted_content",
                table: "Drafts",
                newName: "salt");

            migrationBuilder.RenameColumn(
                name: "content_salt",
                table: "Drafts",
                newName: "iv");

            migrationBuilder.RenameColumn(
                name: "content_iv",
                table: "Drafts",
                newName: "encrypted_data");
        }
    }
}
