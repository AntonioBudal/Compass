using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compass.Modules.Calendar.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduleExceptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScheduleExceptions",
                schema: "calendar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time without time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleExceptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleExceptions_ScheduleProfiles_ScheduleProfileId",
                        column: x => x.ScheduleProfileId,
                        principalSchema: "calendar",
                        principalTable: "ScheduleProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleExceptions_ScheduleProfileId",
                schema: "calendar",
                table: "ScheduleExceptions",
                column: "ScheduleProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduleExceptions",
                schema: "calendar");
        }
    }
}
