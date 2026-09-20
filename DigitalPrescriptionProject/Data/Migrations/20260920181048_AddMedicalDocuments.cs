using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalPrescriptionProject.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicalDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MedicalDocuments",
                columns: table => new
                {
                    MedicalDocumentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    PrescriptionId = table.Column<int>(type: "int", nullable: true),
                    DocumentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalDocuments", x => x.MedicalDocumentId);
                    table.ForeignKey(
                        name: "FK_MedicalDocuments_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicalDocuments_Prescriptions_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalTable: "Prescriptions",
                        principalColumn: "PrescriptionId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedicalDocuments_PatientId",
                table: "MedicalDocuments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalDocuments_PrescriptionId",
                table: "MedicalDocuments",
                column: "PrescriptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedicalDocuments");
        }
    }
}
