using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compass.Modules.Execution.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial_Execution_Schema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "execution");

            migrationBuilder.CreateTable(
                name: "daily_cycles",
                schema: "execution",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_cycles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "execution_logs",
                schema: "execution",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    DailyCycleId = table.Column<Guid>(type: "uuid", nullable: true),
                    EndTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StartTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_execution_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_execution_logs_daily_cycles_DailyCycleId",
                        column: x => x.DailyCycleId,
                        principalSchema: "execution",
                        principalTable: "daily_cycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_execution_logs_DailyCycleId",
                schema: "execution",
                table: "execution_logs",
                column: "DailyCycleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "execution_logs",
                schema: "execution");

            migrationBuilder.DropTable(
                name: "daily_cycles",
                schema: "execution");
        }
    }
}
