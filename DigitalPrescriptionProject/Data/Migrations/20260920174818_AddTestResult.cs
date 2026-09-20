using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalPrescriptionProject.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTestResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TestResults",
                columns: table => new
                {
                    TestResultId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrescribedTestId = table.Column<int>(type: "int", nullable: false),
                    ResultValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferenceRange = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestResults", x => x.TestResultId);
                    table.ForeignKey(
                        name: "FK_TestResults_PrescribedTests_PrescribedTestId",
                        column: x => x.PrescribedTestId,
                        principalTable: "PrescribedTests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestResults_PrescribedTestId",
                table: "TestResults",
                column: "PrescribedTestId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestResults");
        }
    }
}
