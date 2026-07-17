using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compass.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDecisionSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "decision_snapshots",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    available_window_minutes = table.Column<int>(type: "integer", nullable: false),
                    user_energy_context = table.Column<short>(type: "smallint", nullable: false),
                    top1_id = table.Column<Guid>(type: "uuid", nullable: true),
                    top2_id = table.Column<Guid>(type: "uuid", nullable: true),
                    top3_id = table.Column<Guid>(type: "uuid", nullable: true),
                    chosen_commitment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    was_ignored = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
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

            migrationBuilder.CreateIndex(
                name: "idx_decision_snapshots_user_history",
                table: "decision_snapshots",
                columns: new[] { "user_id", "created_at" },
                descending: new[] { false, true });

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "decision_snapshots");
        }
    }
}
