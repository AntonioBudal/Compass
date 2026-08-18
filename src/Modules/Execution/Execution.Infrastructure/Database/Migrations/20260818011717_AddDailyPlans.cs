using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compass.Modules.Execution.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyPlans",
                schema: "execution",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DailyPlanItems",
                schema: "execution",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DailyPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    End = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyPlanItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyPlanItems_DailyPlans_DailyPlanId",
                        column: x => x.DailyPlanId,
                        principalSchema: "execution",
                        principalTable: "DailyPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyPlanItems_DailyPlanId",
                schema: "execution",
                table: "DailyPlanItems",
                column: "DailyPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyPlans_ProfileId_Date",
                schema: "execution",
                table: "DailyPlans",
                columns: new[] { "ProfileId", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyPlanItems",
                schema: "execution");

            migrationBuilder.DropTable(
                name: "DailyPlans",
                schema: "execution");
        }
    }
}
