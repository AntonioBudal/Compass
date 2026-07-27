using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compass.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyReviewsAndSettingsReconciliation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_settings_users_user_id",
                table: "settings");

            migrationBuilder.DropCheckConstraint(
                name: "chk_default_energy_level",
                table: "settings");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "settings",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<string>(
                name: "theme",
                table: "settings",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "dark");

            migrationBuilder.AlterColumn<string>(
                name: "preferences_json",
                table: "settings",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldDefaultValueSql: "'{}'::jsonb");

            migrationBuilder.AlterColumn<short>(
                name: "default_energy_level",
                table: "settings",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldDefaultValue: (short)2);

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "daily_review_time",
                table: "settings",
                type: "time without time zone",
                nullable: false,
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone",
                oldDefaultValueSql: "'20:00:00'");

            migrationBuilder.AlterColumn<bool>(
                name: "auto_postpone_enabled",
                table: "settings",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.CreateTable(
                name: "daily_reviews",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    review_date = table.Column<DateOnly>(type: "date", nullable: false),
                    completed_count = table.Column<int>(type: "integer", nullable: false),
                    postponed_count = table.Column<int>(type: "integer", nullable: false),
                    total_focus_minutes = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_reviews", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_daily_reviews_user_date",
                table: "daily_reviews",
                columns: new[] { "user_id", "review_date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "daily_reviews");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "settings",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "theme",
                table: "settings",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "dark",
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<string>(
                name: "preferences_json",
                table: "settings",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{}'::jsonb",
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<short>(
                name: "default_energy_level",
                table: "settings",
                type: "smallint",
                nullable: false,
                defaultValue: (short)2,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "daily_review_time",
                table: "settings",
                type: "time without time zone",
                nullable: false,
                defaultValueSql: "'20:00:00'",
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone");

            migrationBuilder.AlterColumn<bool>(
                name: "auto_postpone_enabled",
                table: "settings",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddCheckConstraint(
                name: "chk_default_energy_level",
                table: "settings",
                sql: "default_energy_level BETWEEN 1 AND 3");

            migrationBuilder.AddForeignKey(
                name: "FK_settings_users_user_id",
                table: "settings",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
