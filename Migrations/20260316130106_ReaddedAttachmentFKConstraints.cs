using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstralDiaryApi.Migrations
{
    /// <inheritdoc />
    public partial class ReaddedAttachmentFKConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .AlterColumn<string>(
                    name: "entry_id",
                    table: "Attachments",
                    type: "varchar(25)",
                    nullable: true,
                    oldClrType: typeof(string),
                    oldType: "longtext",
                    oldNullable: true
                )
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder
                .AlterColumn<string>(
                    name: "draft_id",
                    table: "Attachments",
                    type: "varchar(25)",
                    nullable: true,
                    oldClrType: typeof(string),
                    oldType: "longtext",
                    oldNullable: true
                )
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Entries_entry_id",
                table: "Entries",
                column: "entry_id"
            );

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Drafts_draft_id",
                table: "Drafts",
                column: "draft_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_draft_id",
                table: "Attachments",
                column: "draft_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_entry_id",
                table: "Attachments",
                column: "entry_id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Drafts_draft_id",
                table: "Attachments",
                column: "draft_id",
                principalTable: "Drafts",
                principalColumn: "draft_id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Entries_entry_id",
                table: "Attachments",
                column: "entry_id",
                principalTable: "Entries",
                principalColumn: "entry_id",
                onDelete: ReferentialAction.Cascade
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Drafts_draft_id",
                table: "Attachments"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Entries_entry_id",
                table: "Attachments"
            );

            migrationBuilder.DropUniqueConstraint(name: "AK_Entries_entry_id", table: "Entries");

            migrationBuilder.DropUniqueConstraint(name: "AK_Drafts_draft_id", table: "Drafts");

            migrationBuilder.DropIndex(name: "IX_Attachments_draft_id", table: "Attachments");

            migrationBuilder.DropIndex(name: "IX_Attachments_entry_id", table: "Attachments");

            migrationBuilder
                .AlterColumn<string>(
                    name: "entry_id",
                    table: "Attachments",
                    type: "longtext",
                    nullable: true,
                    oldClrType: typeof(string),
                    oldType: "varchar(25)",
                    oldNullable: true
                )
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder
                .AlterColumn<string>(
                    name: "draft_id",
                    table: "Attachments",
                    type: "longtext",
                    nullable: true,
                    oldClrType: typeof(string),
                    oldType: "varchar(25)",
                    oldNullable: true
                )
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
