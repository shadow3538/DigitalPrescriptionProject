using DigitalPrescriptionProject.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace DigitalPrescriptionProject.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser, ApplicationRole, string>(options)
    {
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<PrescriptionItem> PrescriptionItems { get; set; }
        public DbSet<PrescribedTest> PrescribedTests { get; set; }
        public DbSet<TestResult> TestResults { get; set; }
        public DbSet<MedicalDocument> MedicalDocuments { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Doctor>().HasData(
                new Doctor
                {
                    DoctorId = 1,
                    FirstName = "John",
                    LastName = "Smith",
                    Speciality = Speciality.GENERAL_PHYSICIAN,
                    Age = 45,
                    Phone = "01711111111",
                    Email = "john.smith@example.com",
                    ImagePath = "/Images/DoctorsImage/doctor1.jpg"
                },

                new Doctor
                {
                    DoctorId = 2,
                    FirstName = "Sarah",
                    LastName = "Williams",
                    Speciality = Speciality.CARDIOLOGY,
                    Age = 52,
                    Phone = "01722222222",
                    Email = "sarah.williams@example.com",
                    ImagePath = "/Images/DoctorsImage/doctor2.jpg"
                },

                new Doctor
                {
                    DoctorId = 3,
                    FirstName = "Michael",
                    LastName = "Brown",
                    Speciality = Speciality.NEUROLOGY,
                    Age = 48,
                    Phone = "01733333333",
                    Email = "michael.brown@example.com",
                    ImagePath = "/Images/DoctorsImage/doctor3.jpg"
                },

                new Doctor
                {
                    DoctorId = 4,
                    FirstName = "Emily",
                    LastName = "Davis",
                    Speciality = Speciality.DERMATOLOGY,
                    Age = 39,
                    Phone = "01744444444",
                    Email = "emily.davis@example.com",
                    ImagePath = "/Images/DoctorsImage/doctor4.jpg"
                },

                new Doctor
                {
                    DoctorId = 5,
                    FirstName = "David",
                    LastName = "Wilson",
                    Speciality = Speciality.PEDIATRICS,
                    Age = 42,
                    Phone = "01755555555",
                    Email = "david.wilson@example.com",
                    ImagePath = "/Images/DoctorsImage/doctor5.jpg"
                },

                new Doctor
                {
                    DoctorId = 6,
                    FirstName = "Olivia",
                    LastName = "Taylor",
                    Speciality = Speciality.GYNECOLOGY,
                    Age = 44,
                    Phone = "01766666666",
                    Email = "olivia.taylor@example.com",
                    ImagePath = "/Images/DoctorsImage/doctor6.jpg"
                },

                new Doctor
                {
                    DoctorId = 7,
                    FirstName = "Robert",
                    LastName = "Anderson",
                    Speciality = Speciality.ORTHOPEDICS,
                    Age = 55,
                    Phone = "01777777777",
                    Email = "robert.anderson@example.com",
                    ImagePath = "/Images/DoctorsImage/doctor7.jpg"
                },

                new Doctor
                {
                    DoctorId = 8,
                    FirstName = "Sophia",
                    LastName = "Thomas",
                    Speciality = Speciality.PSYCHIATRY,
                    Age = 41,
                    Phone = "01788888888",
                    Email = "sophia.thomas@example.com",
                    ImagePath = "/Images/DoctorsImage/doctor8.jpg"
                },

                new Doctor
                {
                    DoctorId = 9,
                    FirstName = "Daniel",
                    LastName = "Moore",
                    Speciality = Speciality.GASTROENTEROLOGY,
                    Age = 50,
                    Phone = "01799999999",
                    Email = "daniel.moore@example.com",
                    ImagePath = "/Images/DoctorsImage/doctor9.jpg"
                },

                new Doctor
                {
                    DoctorId = 10,
                    FirstName = "Emma",
                    LastName = "Martin",
                    Speciality = Speciality.OPHTHALMOLOGY,
                    Age = 37,
                    Phone = "01811111111",
                    Email = "emma.martin@example.com",
                    ImagePath = "/Images/DoctorsImage/doctor10.jpg"
                }
            );



            modelBuilder.Entity<Patient>().HasData(
                new Patient
                {
                    PatientId = 1,
                    FirstName = "Rahim",
                    LastName = "Ahmed",
                    Age = 35,
                    Phone = "01811112222",
                    Email = "rahim.ahmed@example.com",
                    Gender = Gender.Male,
                    CreatedAt = new DateTime(2026, 1, 10),
                    ImagePath = "/Images/PatientsImage/patient1.jpg"
                },

                new Patient
                {
                    PatientId = 2,
                    FirstName = "Nusrat",
                    LastName = "Jahan",
                    Age = 28,
                    Phone = "01822223333",
                    Email = "nusrat.jahan@example.com",
                    Gender = Gender.Female,
                    CreatedAt = new DateTime(2026, 1, 15),
                    ImagePath = "/Images/PatientsImage/patient2.jpg"
                },

                new Patient
                {
                    PatientId = 3,
                    FirstName = "Karim",
                    LastName = "Hossain",
                    Age = 52,
                    Phone = "01833334444",
                    Email = "karim.hossain@example.com",
                    Gender = Gender.Male,
                    CreatedAt = new DateTime(2026, 2, 5),
                    ImagePath = "/Images/PatientsImage/patient3.jpg"
                },

                new Patient
                {
                    PatientId = 4,
                    FirstName = "Mim",
                    LastName = "Akter",
                    Age = 24,
                    Phone = "01844445555",
                    Email = "mim.akter@example.com",
                    Gender = Gender.Female,
                    CreatedAt = new DateTime(2026, 2, 12),
                    ImagePath = "/Images/PatientsImage/patient4.jpg"
                },

                new Patient
                {
                    PatientId = 5,
                    FirstName = "Sakib",
                    LastName = "Hasan",
                    Age = 41,
                    Phone = "01855556666",
                    Email = "sakib.hasan@example.com",
                    Gender = Gender.Male,
                    CreatedAt = new DateTime(2026, 2, 20),
                    ImagePath = "/Images/PatientsImage/patient5.jpg"
                },

                new Patient
                {
                    PatientId = 6,
                    FirstName = "Sumaiya",
                    LastName = "Rahman",
                    Age = 31,
                    Phone = "01866667777",
                    Email = "sumaiya.rahman@example.com",
                    Gender = Gender.Female,
                    CreatedAt = new DateTime(2026, 3, 3),
                    ImagePath = "/Images/PatientsImage/patient6.jpg"
                },

                new Patient
                {
                    PatientId = 7,
                    FirstName = "Imran",
                    LastName = "Kabir",
                    Age = 47,
                    Phone = "01877778888",
                    Email = "imran.kabir@example.com",
                    Gender = Gender.Male,
                    CreatedAt = new DateTime(2026, 3, 10),
                    ImagePath = "/Images/PatientsImage/patient7.jpg"
                },

                new Patient
                {
                    PatientId = 8,
                    FirstName = "Ayesha",
                    LastName = "Sultana",
                    Age = 19,
                    Phone = "01888889999",
                    Email = "ayesha.sultana@example.com",
                    Gender = Gender.Female,
                    CreatedAt = new DateTime(2026, 3, 18),
                    ImagePath = "/Images/PatientsImage/patient8.jpg"
                },

                new Patient
                {
                    PatientId = 9,
                    FirstName = "Tanvir",
                    LastName = "Islam",
                    Age = 60,
                    Phone = "01911112222",
                    Email = "tanvir.islam@example.com",
                    Gender = Gender.Male,
                    CreatedAt = new DateTime(2026, 4, 2),
                    ImagePath = "/Images/PatientsImage/patient9.jpg"
                },

                new Patient
                {
                    PatientId = 10,
                    FirstName = "Fariha",
                    LastName = "Chowdhury",
                    Age = 26,
                    Phone = "01922223333",
                    Email = "fariha.chowdhury@example.com",
                    Gender = Gender.Female,
                    CreatedAt = new DateTime(2026, 4, 10),
                    ImagePath = "/Images/PatientsImage/patient10.jpg"
                },

                new Patient
                {
                    PatientId = 11,
                    FirstName = "Arif",
                    LastName = "Mahmud",
                    Age = 38,
                    Phone = "01933334444",
                    Email = "arif.mahmud@example.com",
                    Gender = Gender.Male,
                    CreatedAt = new DateTime(2026, 4, 15),
                    ImagePath = "/Images/PatientsImage/patient11.jpg"
                },

                new Patient
                {
                    PatientId = 12,
                    FirstName = "Raisa",
                    LastName = "Islam",
                    Age = 33,
                    Phone = "01944445555",
                    Email = "raisa.islam@example.com",
                    Gender = Gender.Female,
                    CreatedAt = new DateTime(2026, 5, 1),
                    ImagePath = "/Images/PatientsImage/patient12.jpg"
                }

            );
            modelBuilder.Entity<Doctor>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Doctor>()
                .HasIndex(a => a.UserId)
                .IsUnique();



            modelBuilder.Entity<Patient>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.SetNull);



            modelBuilder.Entity<Patient>()
                .HasIndex(a => a.UserId)
                .IsUnique();



            modelBuilder.Entity<PrescribedTest>()
                .HasOne(t => t.TestResult)
                .WithOne(r => r.PrescribedTest)
                .HasForeignKey<TestResult>(
                    r => r.PrescribedTestId)
                .OnDelete(DeleteBehavior.Cascade);



            modelBuilder.Entity<MedicalDocument>()
                .HasOne(d => d.Patient)
                .WithMany(p => p.MedicalDocuments)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<MedicalDocument>()
                .HasOne(d => d.Prescription)
                .WithMany(p => p.MedicalDocuments)
                .HasForeignKey(d => d.PrescriptionId)
                .OnDelete(DeleteBehavior.NoAction);
        }

    }

}


