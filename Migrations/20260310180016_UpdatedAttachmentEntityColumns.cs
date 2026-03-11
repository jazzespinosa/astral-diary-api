using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstralDiaryApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedAttachmentEntityColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "file_name",
                table: "Attachments",
                newName: "original_name");

            migrationBuilder.AddColumn<string>(
                name: "internal_name",
                table: "Attachments",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "internal_name",
                table: "Attachments");

            migrationBuilder.RenameColumn(
                name: "original_name",
                table: "Attachments",
                newName: "file_name");
        }
    }
}
