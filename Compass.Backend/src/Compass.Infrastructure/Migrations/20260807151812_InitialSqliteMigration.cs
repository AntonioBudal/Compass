using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compass.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialSqliteMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "daily_reviews",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    user_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    review_date = table.Column<DateOnly>(type: "date", nullable: false),
                    completed_count = table.Column<int>(type: "INTEGER", nullable: false),
                    postponed_count = table.Column<int>(type: "INTEGER", nullable: false),
                    total_focus_minutes = table.Column<int>(type: "INTEGER", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_reviews", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "settings",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    default_energy_level = table.Column<short>(type: "INTEGER", nullable: false),
                    theme = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    auto_postpone_enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    daily_review_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    preferences_json = table.Column<string>(type: "jsonb", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_settings", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "user_scoring_profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    user_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    sample_count = table.Column<int>(type: "INTEGER", nullable: false),
                    urgency_weight_adjust = table.Column<double>(type: "REAL", nullable: false),
                    strategy_weight_adjust = table.Column<double>(type: "REAL", nullable: false),
                    energy_alignment_weight = table.Column<double>(type: "REAL", nullable: false),
                    postponement_penalty_weight = table.Column<double>(type: "REAL", nullable: false),
                    eai_multiplier = table.Column<double>(type: "REAL", nullable: false),
                    morning_energy_bias = table.Column<double>(type: "REAL", nullable: false),
                    afternoon_energy_bias = table.Column<double>(type: "REAL", nullable: false),
                    evening_energy_bias = table.Column<double>(type: "REAL", nullable: false),
                    night_energy_bias = table.Column<double>(type: "REAL", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Version = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_scoring_profiles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    password_hash = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    time_zone = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "America/Sao_Paulo"),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "goals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    user_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    why_description = table.Column<string>(type: "TEXT", nullable: true),
                    target_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    status = table.Column<int>(type: "INTEGER", nullable: false),
                    progress_percentage = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false, defaultValue: 0.00m),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_goals", x => x.id);
                    table.CheckConstraint("chk_goal_progress", "progress_percentage BETWEEN 0.00 AND 100.00");
                    table.CheckConstraint("chk_goal_title_length", "length(title) >= 3");
                    table.ForeignKey(
                        name: "FK_goals_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "schedules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    user_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    day_of_week = table.Column<short>(type: "INTEGER", nullable: false),
                    work_start = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    work_end = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_schedules", x => x.id);
                    table.CheckConstraint("chk_schedule_day", "day_of_week BETWEEN 0 AND 6");
                    table.CheckConstraint("chk_schedule_time_order", "work_end > work_start");
                    table.ForeignKey(
                        name: "FK_schedules_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    user_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    color_hex = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false, defaultValue: "#6366F1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tags", x => x.id);
                    table.CheckConstraint("chk_tag_name_length", "length(name) >= 2");
                    table.ForeignKey(
                        name: "FK_tags_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "projects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    user_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    goal_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    deadline = table.Column<DateTime>(type: "TEXT", nullable: true),
                    status = table.Column<int>(type: "INTEGER", nullable: false),
                    total_estimated_duration_minutes = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_used_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects", x => x.id);
                    table.CheckConstraint("chk_project_duration", "total_estimated_duration_minutes >= 0");
                    table.CheckConstraint("chk_project_title_length", "length(title) >= 3");
                    table.ForeignKey(
                        name: "FK_projects_goals_goal_id",
                        column: x => x.goal_id,
                        principalTable: "goals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_projects_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "commitments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    user_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    project_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    title = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    type = table.Column<int>(type: "INTEGER", nullable: false),
                    status = table.Column<int>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    completed_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    converted_to_commitment_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    best_streak = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    content = table.Column<string>(type: "TEXT", nullable: true),
                    cron_expression = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    current_streak = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    deadline = table.Column<DateTime>(type: "TEXT", nullable: true),
                    end_time = table.Column<DateTime>(type: "TEXT", nullable: true),
                    energy_required = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)2),
                    estimated_duration_minutes = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 30),
                    location_or_link = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    postponed_count = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    start_time = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_commitments", x => x.id);
                    table.CheckConstraint("chk_commitment_title_length", "length(title) >= 3");
                    table.CheckConstraint("chk_event_time_validity", "(type != 'event') OR (start_time IS NOT NULL AND end_time IS NOT NULL AND end_time > start_time)");
                    table.ForeignKey(
                        name: "FK_commitments_commitments_converted_to_commitment_id",
                        column: x => x.converted_to_commitment_id,
                        principalTable: "commitments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_commitments_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_commitments_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "decision_snapshots",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    user_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    available_window_minutes = table.Column<int>(type: "INTEGER", nullable: false),
                    user_energy_context = table.Column<short>(type: "INTEGER", nullable: false),
                    top1_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    top2_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    top3_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    chosen_commitment_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    was_ignored = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_decision_snapshots", x => x.id);
                    table.ForeignKey(
                        name: "FK_decision_snapshots_commitments_chosen_commitment_id",
                        column: x => x.chosen_commitment_id,
                        principalTable: "commitments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_decision_snapshots_commitments_top1_id",
                        column: x => x.top1_id,
                        principalTable: "commitments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_decision_snapshots_commitments_top2_id",
                        column: x => x.top2_id,
                        principalTable: "commitments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_decision_snapshots_commitments_top3_id",
                        column: x => x.top3_id,
                        principalTable: "commitments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_decision_snapshots_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dependencies",
                columns: table => new
                {
                    parent_commitment_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    child_commitment_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dependencies", x => new { x.parent_commitment_id, x.child_commitment_id });
                    table.CheckConstraint("chk_no_self_dependency", "parent_commitment_id != child_commitment_id");
                    table.ForeignKey(
                        name: "FK_dependencies_commitments_child_commitment_id",
                        column: x => x.child_commitment_id,
                        principalTable: "commitments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dependencies_commitments_parent_commitment_id",
                        column: x => x.parent_commitment_id,
                        principalTable: "commitments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "focus_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    user_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    commitment_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    start_time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    end_time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    actual_duration_minutes = table.Column<int>(type: "INTEGER", nullable: false),
                    notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_focus_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_focus_sessions_commitments_commitment_id",
                        column: x => x.commitment_id,
                        principalTable: "commitments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_focus_sessions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reminders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    commitment_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    trigger_time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    is_sent = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reminders", x => x.id);
                    table.ForeignKey(
                        name: "FK_reminders_commitments_commitment_id",
                        column: x => x.commitment_id,
                        principalTable: "commitments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_commitments_events_lookup",
                table: "commitments",
                column: "user_id",
                filter: "type = 'event' AND status != 'archived'");

            migrationBuilder.CreateIndex(
                name: "idx_commitments_now_engine",
                table: "commitments",
                columns: new[] { "user_id", "status", "type" },
                filter: "status IN ('pending', 'in_progress') AND type IN ('task', 'habit')");

            migrationBuilder.CreateIndex(
                name: "idx_commitments_project_id",
                table: "commitments",
                column: "project_id",
                filter: "project_id IS NOT NULL");

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

            migrationBuilder.CreateIndex(
                name: "IX_commitments_converted_to_commitment_id",
                table: "commitments",
                column: "converted_to_commitment_id");

            migrationBuilder.CreateIndex(
                name: "idx_daily_reviews_user_date",
                table: "daily_reviews",
                columns: new[] { "user_id", "review_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_snapshots_reinforcement",
                table: "decision_snapshots",
                columns: new[] { "user_id", "created_at" },
                descending: new[] { false, true },
                filter: "was_ignored = false OR chosen_commitment_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_decision_snapshots_chosen_commitment_id",
                table: "decision_snapshots",
                column: "chosen_commitment_id");

            migrationBuilder.CreateIndex(
                name: "IX_decision_snapshots_top1_id",
                table: "decision_snapshots",
                column: "top1_id");

            migrationBuilder.CreateIndex(
                name: "IX_decision_snapshots_top2_id",
                table: "decision_snapshots",
                column: "top2_id");

            migrationBuilder.CreateIndex(
                name: "IX_decision_snapshots_top3_id",
                table: "decision_snapshots",
                column: "top3_id");

            migrationBuilder.CreateIndex(
                name: "idx_dependencies_child_lookup",
                table: "dependencies",
                column: "child_commitment_id");

            migrationBuilder.CreateIndex(
                name: "idx_focus_sessions_user_time",
                table: "focus_sessions",
                columns: new[] { "user_id", "start_time" },
                filter: "actual_duration_minutes > 0");

            migrationBuilder.CreateIndex(
                name: "IX_focus_sessions_commitment_id",
                table: "focus_sessions",
                column: "commitment_id");

            migrationBuilder.CreateIndex(
                name: "IX_goals_user_id",
                table: "goals",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_projects_user_catalog_lru",
                table: "projects",
                columns: new[] { "user_id", "status", "last_used_at" },
                descending: new[] { false, false, true },
                filter: "status != 'completed' AND status != 'archived'");

            migrationBuilder.CreateIndex(
                name: "idx_projects_user_status",
                table: "projects",
                columns: new[] { "user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_projects_goal_id",
                table: "projects",
                column: "goal_id");

            migrationBuilder.CreateIndex(
                name: "idx_reminders_unsent_trigger",
                table: "reminders",
                column: "trigger_time",
                filter: "is_sent = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_reminders_commitment_id",
                table: "reminders",
                column: "commitment_id");

            migrationBuilder.CreateIndex(
                name: "IX_schedules_user_id",
                table: "schedules",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tags_user_id_name",
                table: "tags",
                columns: new[] { "user_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_user_scoring_profiles_user_id",
                table: "user_scoring_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "daily_reviews");

            migrationBuilder.DropTable(
                name: "decision_snapshots");

            migrationBuilder.DropTable(
                name: "dependencies");

            migrationBuilder.DropTable(
                name: "focus_sessions");

            migrationBuilder.DropTable(
                name: "reminders");

            migrationBuilder.DropTable(
                name: "schedules");

            migrationBuilder.DropTable(
                name: "settings");

            migrationBuilder.DropTable(
                name: "tags");

            migrationBuilder.DropTable(
                name: "user_scoring_profiles");

            migrationBuilder.DropTable(
                name: "commitments");

            migrationBuilder.DropTable(
                name: "projects");

            migrationBuilder.DropTable(
                name: "goals");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
