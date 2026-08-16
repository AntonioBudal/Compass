using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compass.Modules.Calendar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial_Calendar_Schema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "calendar");

            migrationBuilder.CreateTable(
                name: "ScheduleProfiles",
                schema: "calendar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Timezone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleWindows",
                schema: "calendar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<string>(type: "text", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleWindows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleWindows_ScheduleProfiles_ScheduleProfileId",
                        column: x => x.ScheduleProfileId,
                        principalSchema: "calendar",
                        principalTable: "ScheduleProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleWindows_ScheduleProfileId",
                schema: "calendar",
                table: "ScheduleWindows",
                column: "ScheduleProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduleWindows",
                schema: "calendar");

            migrationBuilder.DropTable(
                name: "ScheduleProfiles",
                schema: "calendar");
        }
    }
}
