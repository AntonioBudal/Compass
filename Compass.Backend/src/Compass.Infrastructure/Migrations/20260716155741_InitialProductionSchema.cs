using System;
using Compass.Domain.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compass.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialProductionSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:commitment_status", "archived,blocked,completed,in_progress,pending")
                .Annotation("Npgsql:Enum:commitment_type", "event,habit,note,task")
                .Annotation("Npgsql:Enum:goal_status", "active,cancelled,completed,on_hold");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    time_zone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "America/Sao_Paulo"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "goals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    why_description = table.Column<string>(type: "text", nullable: true),
                    target_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<GoalStatus>(type: "goal_status", nullable: false),
                    progress_percentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0.00m),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_goals", x => x.id);
                    table.CheckConstraint("chk_goal_progress", "progress_percentage BETWEEN 0.00 AND 100.00");
                    table.CheckConstraint("chk_goal_title_length", "char_length(title) >= 3");
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
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    day_of_week = table.Column<short>(type: "smallint", nullable: false),
                    work_start = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    work_end = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
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
                name: "settings",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    default_energy_level = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)2),
                    theme = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "dark"),
                    auto_postpone_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    daily_review_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false, defaultValueSql: "'20:00:00'"),
                    preferences_json = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_settings", x => x.user_id);
                    table.CheckConstraint("chk_default_energy_level", "default_energy_level BETWEEN 1 AND 3");
                    table.ForeignKey(
                        name: "FK_settings_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    color_hex = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false, defaultValue: "#6366F1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tags", x => x.id);
                    table.CheckConstraint("chk_tag_color_hex", "color_hex ~* '^#[a-f0-9]{6}$'");
                    table.CheckConstraint("chk_tag_name_length", "char_length(name) >= 2");
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
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    goal_id = table.Column<Guid>(type: "uuid", nullable: true),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<CommitmentStatus>(type: "commitment_status", nullable: false),
                    total_estimated_duration_minutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects", x => x.id);
                    table.CheckConstraint("chk_project_duration", "total_estimated_duration_minutes >= 0");
                    table.CheckConstraint("chk_project_title_length", "char_length(title) >= 3");
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
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: true),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    type = table.Column<CommitmentType>(type: "commitment_type", nullable: false),
                    status = table.Column<CommitmentStatus>(type: "commitment_status", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    converted_to_commitment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    best_streak = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    content = table.Column<string>(type: "text", nullable: true),
                    cron_expression = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    current_streak = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    end_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    energy_required = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)2),
                    estimated_duration_minutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 30),
                    location_or_link = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    postponed_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_commitments", x => x.id);
                    table.CheckConstraint("chk_commitment_title_length", "char_length(title) >= 3");
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
                name: "dependencies",
                columns: table => new
                {
                    parent_commitment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    child_commitment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
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
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    commitment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    actual_duration_minutes = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_focus_sessions", x => x.id);
                    table.CheckConstraint("chk_focus_duration", "actual_duration_minutes > 0");
                    table.CheckConstraint("chk_focus_time_validity", "end_time > start_time");
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
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    commitment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    trigger_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_sent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
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
                name: "IX_commitments_converted_to_commitment_id",
                table: "commitments",
                column: "converted_to_commitment_id");

            migrationBuilder.CreateIndex(
                name: "idx_dependencies_child_lookup",
                table: "dependencies",
                column: "child_commitment_id");

            migrationBuilder.CreateIndex(
                name: "idx_focus_sessions_user_history",
                table: "focus_sessions",
                columns: new[] { "user_id", "start_time" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_focus_sessions_commitment_id",
                table: "focus_sessions",
                column: "commitment_id");

            migrationBuilder.CreateIndex(
                name: "IX_goals_user_id",
                table: "goals",
                column: "user_id");

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
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
