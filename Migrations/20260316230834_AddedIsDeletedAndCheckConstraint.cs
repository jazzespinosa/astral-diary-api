using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstralDiaryApi.Migrations
{
    /// <inheritdoc />
    public partial class AddedIsDeletedAndCheckConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "Entries",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Entry_DeletedAt_OnlyIf_IsDeleted",
                table: "Entries",
                sql: "(is_deleted = 1 AND deleted_at IS NOT NULL) OR (is_deleted = 0 AND deleted_at IS NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Entry_DeletedAt_OnlyIf_IsDeleted",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "Entries");
        }
    }
}
