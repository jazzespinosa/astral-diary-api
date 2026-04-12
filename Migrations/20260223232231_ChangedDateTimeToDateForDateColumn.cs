using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstralDiaryApi.Migrations
{
    /// <inheritdoc />
    public partial class ChangedDateTimeToDateForDateColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateOnly>(
                name: "date",
                table: "Entries",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp"
            );

            migrationBuilder.AlterColumn<DateOnly>(
                name: "date",
                table: "Drafts",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "date",
                table: "Entries",
                type: "timestamp",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date"
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "date",
                table: "Drafts",
                type: "timestamp",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date"
            );
        }
    }
}
