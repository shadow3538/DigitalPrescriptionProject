using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalPrescriptionProject.Models
{
    public class Doctor
    {
        [Key]
        public int DoctorId { get; set; }
        public string? UserId { get; set; }

        [Required]
        [DisplayName("First Name")]
        public string FirstName { get; set; } = default!;

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = default!;

        [NotMapped]
        [Display(Name ="Name")]
        public string FullName => FirstName + " " + LastName;

        public Speciality Speciality { get; set; }

        [Range(25, 100)]
        public int Age { get; set; }


        public string Phone { get; set; } = default!;

        [EmailAddress]
        public string? Email { get; set; }

        [Display(Name = "Image")]
        public string? ImagePath { get; set; }

        [NotMapped]
        public IFormFile? Upload { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }

        public List<Prescription> Prescriptions { get; set; } = new();

        public void SaveDoctorImage(IWebHostEnvironment env)
        {
            if (Upload is null || Upload.Length == 0)
                return;

            const long maxFileSize = 2 * 1024 * 1024; 

            if (Upload.Length > maxFileSize)
                throw new InvalidOperationException("Image size cannot exceed 2 MB.");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            var extension = Path.GetExtension(Upload.FileName)
                .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                throw new InvalidOperationException("Only JPG, JPEG, PNG and WEBP images are allowed.");

            var folderPath = Path.Combine(
                env.WebRootPath,
                "Images",
                "DoctorsImage");

            Directory.CreateDirectory(folderPath);

            var newFileName = $"{Guid.NewGuid():N}{extension}";

            var physicalPath = Path.Combine(folderPath, newFileName);

            using Stream stream = File.Create(physicalPath);
            Upload.CopyTo(stream);

            ImagePath = $"/Images/DoctorsImage/{newFileName}";
        }

    }

    public enum Speciality
    {
        GENERAL_PHYSICIAN, INTERNAL_MEDICINE, CARDIOLOGY, NEUROLOGY,DERMATOLOGY, PEDIATRICS, PSYCHIATRY, ORTHOPEDICS, ENDOCRINOLOGY, GASTROENTEROLOGY, NEPHROLOGY, UROLOGY, GYNECOLOGY, OBSTETRICS, ENT, OPHTHALMOLOGY, DENTISTRY, PULMONOLOGY, ONCOLOGY, GENERAL_SURGERY, PLASTIC_SURGERY, ANESTHESIOLOGY, EMERGENCY_MEDICINE, RADIOLOGY, PATHOLOGY
    }
}
