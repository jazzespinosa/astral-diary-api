using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstralDiaryApi.Migrations
{
    /// <inheritdoc />
    public partial class RemovedAttachmentAndCombineItToEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Attachments");

            migrationBuilder.DropUniqueConstraint(name: "AK_Entries_entry_id", table: "Entries");

            migrationBuilder.DropUniqueConstraint(name: "AK_Drafts_draft_id", table: "Drafts");

            migrationBuilder
                .AddColumn<string>(
                    name: "att_file_path",
                    table: "Entries",
                    type: "longtext",
                    nullable: true
                )
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder
                .AddColumn<string>(
                    name: "att_thumbnail_path",
                    table: "Entries",
                    type: "longtext",
                    nullable: true
                )
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder
                .AddColumn<string>(
                    name: "attachment_id",
                    table: "Entries",
                    type: "longtext",
                    nullable: true
                )
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder
                .AddColumn<string>(
                    name: "att_file_path",
                    table: "Drafts",
                    type: "longtext",
                    nullable: true
                )
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder
                .AddColumn<string>(
                    name: "att_thumbnail_path",
                    table: "Drafts",
                    type: "longtext",
                    nullable: true
                )
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder
                .AddColumn<string>(
                    name: "attachment_id",
                    table: "Drafts",
                    type: "longtext",
                    nullable: true
                )
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "att_file_path", table: "Entries");

            migrationBuilder.DropColumn(name: "att_thumbnail_path", table: "Entries");

            migrationBuilder.DropColumn(name: "attachment_id", table: "Entries");

            migrationBuilder.DropColumn(name: "att_file_path", table: "Drafts");

            migrationBuilder.DropColumn(name: "att_thumbnail_path", table: "Drafts");

            migrationBuilder.DropColumn(name: "attachment_id", table: "Drafts");

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

            migrationBuilder
                .CreateTable(
                    name: "Attachments",
                    columns: table => new
                    {
                        id = table
                            .Column<int>(type: "int", nullable: false)
                            .Annotation(
                                "MySql:ValueGenerationStrategy",
                                MySqlValueGenerationStrategy.IdentityColumn
                            ),
                        draft_id = table
                            .Column<string>(type: "varchar(25)", nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        entry_id = table
                            .Column<string>(type: "varchar(25)", nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        content_hash = table
                            .Column<string>(type: "longtext", nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                        file_path = table
                            .Column<string>(type: "longtext", nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        internal_name = table
                            .Column<string>(type: "varchar(255)", nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        original_name = table
                            .Column<string>(type: "longtext", nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        thumbnail_path = table
                            .Column<string>(type: "longtext", nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_Attachments", x => x.id);
                        table.CheckConstraint(
                            "CK_Attachment_SingleSource",
                            "(entry_id IS NULL AND draft_id IS NOT NULL) OR (entry_id IS NOT NULL AND draft_id IS NULL)"
                        );
                        table.ForeignKey(
                            name: "FK_Attachments_Drafts_draft_id",
                            column: x => x.draft_id,
                            principalTable: "Drafts",
                            principalColumn: "draft_id",
                            onDelete: ReferentialAction.Cascade
                        );
                        table.ForeignKey(
                            name: "FK_Attachments_Entries_entry_id",
                            column: x => x.entry_id,
                            principalTable: "Entries",
                            principalColumn: "entry_id",
                            onDelete: ReferentialAction.Cascade
                        );
                    }
                )
                .Annotation("MySql:CharSet", "utf8mb4");

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

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_internal_name",
                table: "Attachments",
                column: "internal_name"
            );
        }
    }
}
