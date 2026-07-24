using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compass.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserScoringProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_decision_snapshots_user_history",
                table: "decision_snapshots");

            migrationBuilder.CreateTable(
                name: "user_scoring_profiles",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    eai_multiplier = table.Column<double>(type: "double precision", nullable: false, defaultValue: 1.0),
                    morning_energy_bias = table.Column<double>(type: "double precision", nullable: false, defaultValue: 1.0),
                    afternoon_energy_bias = table.Column<double>(type: "double precision", nullable: false, defaultValue: 1.0),
                    evening_energy_bias = table.Column<double>(type: "double precision", nullable: false, defaultValue: 1.0),
                    urgency_weight_adjust = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.0),
                    strategy_weight_adjust = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.0),
                    sample_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_scoring_profiles", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_user_scoring_profiles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_snapshots_reinforcement",
                table: "decision_snapshots",
                columns: new[] { "user_id", "created_at" },
                descending: new[] { false, true },
                filter: "was_ignored = false OR chosen_commitment_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_scoring_profiles");

            migrationBuilder.DropIndex(
                name: "idx_snapshots_reinforcement",
                table: "decision_snapshots");

            migrationBuilder.CreateIndex(
                name: "idx_decision_snapshots_user_history",
                table: "decision_snapshots",
                columns: new[] { "user_id", "created_at" },
                descending: new[] { false, true });
        }
    }
}
