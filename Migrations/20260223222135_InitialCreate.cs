using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstralDiaryApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase().Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder
                .CreateTable(
                    name: "Users",
                    columns: table => new
                    {
                        id = table
                            .Column<int>(type: "int", nullable: false)
                            .Annotation(
                                "MySql:ValueGenerationStrategy",
                                MySqlValueGenerationStrategy.IdentityColumn
                            ),
                        user_id = table.Column<Guid>(
                            type: "char(36)",
                            nullable: false,
                            collation: "ascii_general_ci"
                        ),
                        email = table
                            .Column<string>(type: "longtext", nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        name = table
                            .Column<string>(type: "longtext", nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        firebase_uid = table
                            .Column<string>(type: "longtext", nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_Users", x => x.id);
                        table.UniqueConstraint("AK_Users_user_id", x => x.user_id);
                    }
                )
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder
                .CreateTable(
                    name: "Draft",
                    columns: table => new
                    {
                        id = table
                            .Column<int>(type: "int", nullable: false)
                            .Annotation(
                                "MySql:ValueGenerationStrategy",
                                MySqlValueGenerationStrategy.IdentityColumn
                            ),
                        draft_id = table
                            .Column<string>(type: "varchar(255)", nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        user_id = table.Column<Guid>(
                            type: "char(36)",
                            nullable: false,
                            collation: "ascii_general_ci"
                        ),
                        created_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                        modified_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                        date = table.Column<DateTime>(type: "timestamp", nullable: false),
                        title = table
                            .Column<string>(type: "longtext", nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        content = table
                            .Column<string>(type: "longtext", nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_Draft", x => x.id);
                        table.UniqueConstraint("AK_Draft_draft_id", x => x.draft_id);
                        table.ForeignKey(
                            name: "FK_Draft_Users_user_id",
                            column: x => x.user_id,
                            principalTable: "Users",
                            principalColumn: "user_id",
                            onDelete: ReferentialAction.Cascade
                        );
                    }
                )
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder
                .CreateTable(
                    name: "Entries",
                    columns: table => new
                    {
                        id = table
                            .Column<int>(type: "int", nullable: false)
                            .Annotation(
                                "MySql:ValueGenerationStrategy",
                                MySqlValueGenerationStrategy.IdentityColumn
                            ),
                        entry_id = table
                            .Column<string>(type: "varchar(255)", nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        user_id = table.Column<Guid>(
                            type: "char(36)",
                            nullable: false,
                            collation: "ascii_general_ci"
                        ),
                        created_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                        modified_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                        date = table.Column<DateTime>(type: "timestamp", nullable: false),
                        title = table
                            .Column<string>(type: "longtext", nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        content = table
                            .Column<string>(type: "longtext", nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        published_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                        deleted_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_Entries", x => x.id);
                        table.UniqueConstraint("AK_Entries_entry_id", x => x.entry_id);
                        table.ForeignKey(
                            name: "FK_Entries_Users_user_id",
                            column: x => x.user_id,
                            principalTable: "Users",
                            principalColumn: "user_id",
                            onDelete: ReferentialAction.Cascade
                        );
                    }
                )
                .Annotation("MySql:CharSet", "utf8mb4");

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
                        entry_id = table
                            .Column<string>(type: "varchar(255)", nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        draft_id = table
                            .Column<string>(type: "varchar(255)", nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        file_name = table
                            .Column<string>(type: "longtext", nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        object_storage_key = table
                            .Column<string>(type: "longtext", nullable: true)
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
                            name: "FK_Attachments_Draft_draft_id",
                            column: x => x.draft_id,
                            principalTable: "Draft",
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
                name: "IX_Draft_draft_id",
                table: "Draft",
                column: "draft_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Draft_modified_at",
                table: "Draft",
                column: "modified_at"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Draft_user_id",
                table: "Draft",
                column: "user_id"
            );

            migrationBuilder.CreateIndex(name: "IX_Entries_date", table: "Entries", column: "date");

            migrationBuilder.CreateIndex(
                name: "IX_Entries_entry_id",
                table: "Entries",
                column: "entry_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Entries_user_id",
                table: "Entries",
                column: "user_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Users_user_id",
                table: "Users",
                column: "user_id",
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Attachments");

            migrationBuilder.DropTable(name: "Draft");

            migrationBuilder.DropTable(name: "Entries");

            migrationBuilder.DropTable(name: "Users");
        }
    }
}
