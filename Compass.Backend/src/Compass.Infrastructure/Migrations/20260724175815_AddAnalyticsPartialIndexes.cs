using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compass.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalyticsPartialIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_focus_sessions_user_history",
                table: "focus_sessions");

            migrationBuilder.DropCheckConstraint(
                name: "chk_focus_duration",
                table: "focus_sessions");

            migrationBuilder.DropCheckConstraint(
                name: "chk_focus_time_validity",
                table: "focus_sessions");

            migrationBuilder.AlterColumn<string>(
                name: "notes",
                table: "focus_sessions",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_focus_sessions_user_time",
                table: "focus_sessions",
                columns: new[] { "user_id", "start_time" },
                filter: "actual_duration_minutes > 0");

            migrationBuilder.CreateIndex(
                name: "idx_commitments_user_postponed",
                table: "commitments",
                columns: new[] { "user_id", "postponed_count", "type", "energy_required" },
                filter: "postponed_count > 0 AND status != 'archived'");

            migrationBuilder.CreateIndex(
                name: "idx_commitments_user_status_completed",
                table: "commitments",
                columns: new[] { "user_id", "status", "completed_at" },
                filter: "status = 'completed' AND completed_at IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_focus_sessions_user_time",
                table: "focus_sessions");

            migrationBuilder.DropIndex(
                name: "idx_commitments_user_postponed",
                table: "commitments");

            migrationBuilder.DropIndex(
                name: "idx_commitments_user_status_completed",
                table: "commitments");

            migrationBuilder.AlterColumn<string>(
                name: "notes",
                table: "focus_sessions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_focus_sessions_user_history",
                table: "focus_sessions",
                columns: new[] { "user_id", "start_time" },
                descending: new[] { false, true });

            migrationBuilder.AddCheckConstraint(
                name: "chk_focus_duration",
                table: "focus_sessions",
                sql: "actual_duration_minutes > 0");

            migrationBuilder.AddCheckConstraint(
                name: "chk_focus_time_validity",
                table: "focus_sessions",
                sql: "end_time > start_time");
        }
    }
}
