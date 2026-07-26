using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compass.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdaptiveScoringProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_scoring_profiles_users_user_id",
                table: "user_scoring_profiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_scoring_profiles",
                table: "user_scoring_profiles");

            migrationBuilder.AlterColumn<double>(
                name: "urgency_weight_adjust",
                table: "user_scoring_profiles",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldDefaultValue: 0.0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "user_scoring_profiles",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<double>(
                name: "strategy_weight_adjust",
                table: "user_scoring_profiles",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldDefaultValue: 0.0);

            migrationBuilder.AlterColumn<int>(
                name: "sample_count",
                table: "user_scoring_profiles",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<double>(
                name: "morning_energy_bias",
                table: "user_scoring_profiles",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldDefaultValue: 1.0);

            migrationBuilder.AlterColumn<double>(
                name: "evening_energy_bias",
                table: "user_scoring_profiles",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldDefaultValue: 1.0);

            migrationBuilder.AlterColumn<double>(
                name: "eai_multiplier",
                table: "user_scoring_profiles",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldDefaultValue: 1.0);

            migrationBuilder.AlterColumn<double>(
                name: "afternoon_energy_bias",
                table: "user_scoring_profiles",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldDefaultValue: 1.0);

            migrationBuilder.AddColumn<Guid>(
                name: "id",
                table: "user_scoring_profiles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<double>(
                name: "energy_alignment_weight",
                table: "user_scoring_profiles",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "night_energy_bias",
                table: "user_scoring_profiles",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "postponement_penalty_weight",
                table: "user_scoring_profiles",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_used_at",
                table: "projects",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_scoring_profiles",
                table: "user_scoring_profiles",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_user_scoring_profiles_user_id",
                table: "user_scoring_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_projects_user_catalog_lru",
                table: "projects",
                columns: new[] { "user_id", "status", "last_used_at" },
                descending: new[] { false, false, true },
                filter: "status != 'completed' AND status != 'archived'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_user_scoring_profiles",
                table: "user_scoring_profiles");

            migrationBuilder.DropIndex(
                name: "idx_user_scoring_profiles_user_id",
                table: "user_scoring_profiles");

            migrationBuilder.DropIndex(
                name: "idx_projects_user_catalog_lru",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "id",
                table: "user_scoring_profiles");

            migrationBuilder.DropColumn(
                name: "energy_alignment_weight",
                table: "user_scoring_profiles");

            migrationBuilder.DropColumn(
                name: "night_energy_bias",
                table: "user_scoring_profiles");

            migrationBuilder.DropColumn(
                name: "postponement_penalty_weight",
                table: "user_scoring_profiles");

            migrationBuilder.DropColumn(
                name: "last_used_at",
                table: "projects");

            migrationBuilder.AlterColumn<double>(
                name: "urgency_weight_adjust",
                table: "user_scoring_profiles",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "user_scoring_profiles",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<double>(
                name: "strategy_weight_adjust",
                table: "user_scoring_profiles",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<int>(
                name: "sample_count",
                table: "user_scoring_profiles",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<double>(
                name: "morning_energy_bias",
                table: "user_scoring_profiles",
                type: "double precision",
                nullable: false,
                defaultValue: 1.0,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<double>(
                name: "evening_energy_bias",
                table: "user_scoring_profiles",
                type: "double precision",
                nullable: false,
                defaultValue: 1.0,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<double>(
                name: "eai_multiplier",
                table: "user_scoring_profiles",
                type: "double precision",
                nullable: false,
                defaultValue: 1.0,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<double>(
                name: "afternoon_energy_bias",
                table: "user_scoring_profiles",
                type: "double precision",
                nullable: false,
                defaultValue: 1.0,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_scoring_profiles",
                table: "user_scoring_profiles",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_user_scoring_profiles_users_user_id",
                table: "user_scoring_profiles",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
