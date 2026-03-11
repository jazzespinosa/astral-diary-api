using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstralDiaryApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedDraftConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Draft_draft_id",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Draft_Users_user_id",
                table: "Draft");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Draft_draft_id",
                table: "Draft");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Draft",
                table: "Draft");

            migrationBuilder.RenameTable(
                name: "Draft",
                newName: "Drafts");

            migrationBuilder.RenameIndex(
                name: "IX_Draft_user_id",
                table: "Drafts",
                newName: "IX_Drafts_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_Draft_modified_at",
                table: "Drafts",
                newName: "IX_Drafts_modified_at");

            migrationBuilder.RenameIndex(
                name: "IX_Draft_draft_id",
                table: "Drafts",
                newName: "IX_Drafts_draft_id");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Drafts_draft_id",
                table: "Drafts",
                column: "draft_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Drafts",
                table: "Drafts",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Drafts_draft_id",
                table: "Attachments",
                column: "draft_id",
                principalTable: "Drafts",
                principalColumn: "draft_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Drafts_Users_user_id",
                table: "Drafts",
                column: "user_id",
                principalTable: "Users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Drafts_draft_id",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Drafts_Users_user_id",
                table: "Drafts");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Drafts_draft_id",
                table: "Drafts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Drafts",
                table: "Drafts");

            migrationBuilder.RenameTable(
                name: "Drafts",
                newName: "Draft");

            migrationBuilder.RenameIndex(
                name: "IX_Drafts_user_id",
                table: "Draft",
                newName: "IX_Draft_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_Drafts_modified_at",
                table: "Draft",
                newName: "IX_Draft_modified_at");

            migrationBuilder.RenameIndex(
                name: "IX_Drafts_draft_id",
                table: "Draft",
                newName: "IX_Draft_draft_id");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Draft_draft_id",
                table: "Draft",
                column: "draft_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Draft",
                table: "Draft",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Draft_draft_id",
                table: "Attachments",
                column: "draft_id",
                principalTable: "Draft",
                principalColumn: "draft_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Draft_Users_user_id",
                table: "Draft",
                column: "user_id",
                principalTable: "Users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
