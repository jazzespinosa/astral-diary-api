using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstralDiaryApi.Migrations
{
    /// <inheritdoc />
    public partial class AddedEncryptedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "encrypted_data",
                table: "Entries",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "iv",
                table: "Entries",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "salt",
                table: "Entries",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "encrypted_data",
                table: "Drafts",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "iv",
                table: "Drafts",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "salt",
                table: "Drafts",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "encrypted_data",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "iv",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "salt",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "encrypted_data",
                table: "Drafts");

            migrationBuilder.DropColumn(
                name: "iv",
                table: "Drafts");

            migrationBuilder.DropColumn(
                name: "salt",
                table: "Drafts");
        }
    }
}
