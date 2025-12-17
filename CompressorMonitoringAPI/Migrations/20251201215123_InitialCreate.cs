using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CompressorMonitoringAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Equipment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ProcessStage = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    InstallationDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EquipmentSpecifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EquipmentId = table.Column<int>(type: "INTEGER", nullable: false),
                    ParameterName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ParameterValue = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ParameterUnit = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentSpecifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EquipmentSpecifications_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MonitoringReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EquipmentId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReportDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OperatorName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Shift = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ReportType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Conclusions = table.Column<string>(type: "TEXT", nullable: false),
                    HealthScore = table.Column<double>(type: "REAL", nullable: false),
                    HealthStatus = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitoringReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonitoringReports_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportParameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReportId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Value = table.Column<double>(type: "REAL", nullable: false),
                    Unit = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    WarningLimit = table.Column<double>(type: "REAL", nullable: false),
                    CriticalLimit = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportParameters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportParameters_MonitoringReports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "MonitoringReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentSpecifications_EquipmentId",
                table: "EquipmentSpecifications",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_MonitoringReports_EquipmentId_ReportDate",
                table: "MonitoringReports",
                columns: new[] { "EquipmentId", "ReportDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportParameters_ReportId",
                table: "ReportParameters",
                column: "ReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EquipmentSpecifications");

            migrationBuilder.DropTable(
                name: "ReportParameters");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "MonitoringReports");

            migrationBuilder.DropTable(
                name: "Equipment");
        }
    }
}
