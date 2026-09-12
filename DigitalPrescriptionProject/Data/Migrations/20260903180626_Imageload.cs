using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalPrescriptionProject.Data.Migrations
{
    /// <inheritdoc />
    public partial class Imageload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Doctors");
        }
    }
}
