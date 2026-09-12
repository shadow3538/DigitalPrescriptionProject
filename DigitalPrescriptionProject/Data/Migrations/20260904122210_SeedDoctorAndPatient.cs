using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DigitalPrescriptionProject.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedDoctorAndPatient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Doctors",
                columns: new[] { "DoctorId", "Age", "Email", "FirstName", "ImagePath", "LastName", "Phone", "Speciality" },
                values: new object[,]
                {
                    { 1, 45, "john.smith@example.com", "John", "/Images/DoctorsImage/doctor1.jpg", "Smith", "01711111111", 0 },
                    { 2, 52, "sarah.williams@example.com", "Sarah", "/Images/DoctorsImage/doctor2.jpg", "Williams", "01722222222", 2 },
                    { 3, 48, "michael.brown@example.com", "Michael", "/Images/DoctorsImage/doctor3.jpg", "Brown", "01733333333", 3 },
                    { 4, 39, "emily.davis@example.com", "Emily", "/Images/DoctorsImage/doctor4.jpg", "Davis", "01744444444", 4 },
                    { 5, 42, "david.wilson@example.com", "David", "/Images/DoctorsImage/doctor5.jpg", "Wilson", "01755555555", 5 },
                    { 6, 44, "olivia.taylor@example.com", "Olivia", "/Images/DoctorsImage/doctor6.jpg", "Taylor", "01766666666", 12 },
                    { 7, 55, "robert.anderson@example.com", "Robert", "/Images/DoctorsImage/doctor7.jpg", "Anderson", "01777777777", 7 },
                    { 8, 41, "sophia.thomas@example.com", "Sophia", "/Images/DoctorsImage/doctor8.jpg", "Thomas", "01788888888", 6 },
                    { 9, 50, "daniel.moore@example.com", "Daniel", "/Images/DoctorsImage/doctor9.jpg", "Moore", "01799999999", 9 },
                    { 10, 37, "emma.martin@example.com", "Emma", "/Images/DoctorsImage/doctor10.jpg", "Martin", "01811111111", 15 }
                });

            migrationBuilder.InsertData(
                table: "Patients",
                columns: new[] { "PatientId", "Age", "CreatedAt", "Email", "FirstName", "Gender", "ImagePath", "LastName", "Phone" },
                values: new object[,]
                {
                    { 1, 35, new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "rahim.ahmed@example.com", "Rahim", 0, "/Images/PatientsImage/patient1.jpg", "Ahmed", "01811112222" },
                    { 2, 28, new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "nusrat.jahan@example.com", "Nusrat", 1, "/Images/PatientsImage/patient2.jpg", "Jahan", "01822223333" },
                    { 3, 52, new DateTime(2026, 2, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "karim.hossain@example.com", "Karim", 0, "/Images/PatientsImage/patient3.jpg", "Hossain", "01833334444" },
                    { 4, 24, new DateTime(2026, 2, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "mim.akter@example.com", "Mim", 1, "/Images/PatientsImage/patient4.jpg", "Akter", "01844445555" },
                    { 5, 41, new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "sakib.hasan@example.com", "Sakib", 0, "/Images/PatientsImage/patient5.jpg", "Hasan", "01855556666" },
                    { 6, 31, new DateTime(2026, 3, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "sumaiya.rahman@example.com", "Sumaiya", 1, "/Images/PatientsImage/patient6.jpg", "Rahman", "01866667777" },
                    { 7, 47, new DateTime(2026, 3, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "imran.kabir@example.com", "Imran", 0, "/Images/PatientsImage/patient7.jpg", "Kabir", "01877778888" },
                    { 8, 19, new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "ayesha.sultana@example.com", "Ayesha", 1, "/Images/PatientsImage/patient8.jpg", "Sultana", "01888889999" },
                    { 9, 60, new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "tanvir.islam@example.com", "Tanvir", 0, "/Images/PatientsImage/patient9.jpg", "Islam", "01911112222" },
                    { 10, 26, new DateTime(2026, 4, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "fariha.chowdhury@example.com", "Fariha", 1, "/Images/PatientsImage/patient10.jpg", "Chowdhury", "01922223333" },
                    { 11, 38, new DateTime(2026, 4, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "arif.mahmud@example.com", "Arif", 0, "/Images/PatientsImage/patient11.jpg", "Mahmud", "01933334444" },
                    { 12, 33, new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "raisa.islam@example.com", "Raisa", 1, "/Images/PatientsImage/patient12.jpg", "Islam", "01944445555" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "DoctorId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "DoctorId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "DoctorId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "DoctorId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "DoctorId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "DoctorId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "DoctorId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "DoctorId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "DoctorId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "DoctorId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "PatientId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "PatientId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "PatientId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "PatientId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "PatientId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "PatientId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "PatientId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "PatientId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "PatientId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "PatientId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "PatientId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "PatientId",
                keyValue: 12);
        }
    }
}
