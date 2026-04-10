using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstralDiaryApi.Migrations
{
    /// <inheritdoc />
    public partial class RemovedTitleContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Entries_title_content",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "att_iv",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "att_salt",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "content",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "thumbnail_iv",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "thumbnail_salt",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "title",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "att_iv",
                table: "Drafts");

            migrationBuilder.DropColumn(
                name: "att_salt",
                table: "Drafts");

            migrationBuilder.DropColumn(
                name: "content",
                table: "Drafts");

            migrationBuilder.DropColumn(
                name: "thumbnail_iv",
                table: "Drafts");

            migrationBuilder.DropColumn(
                name: "thumbnail_salt",
                table: "Drafts");

            migrationBuilder.DropColumn(
                name: "title",
                table: "Drafts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "content",
                table: "Entries",
                type: "TEXT",
                nullable: false)
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
                name: "title",
                table: "Entries",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "")
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
                name: "content",
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

            migrationBuilder.AddColumn<string>(
                name: "title",
                table: "Drafts",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Entries_title_content",
                table: "Entries",
                columns: new[] { "title", "content" })
                .Annotation("MySql:FullTextIndex", true);
        }
    }
}
