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

        public List<Prescription> Prescriptions { get; set; } = new();

        public void SaveDoctorImage(IWebHostEnvironment env)
        {
            if (Upload is not null)
            {
                var fileName = $"/Images/DoctorsImage/_{Guid.NewGuid()}_{Upload.FileName}";
                using Stream stream = File.Create(env.WebRootPath + fileName);
                Upload.CopyTo(stream);
                ImagePath = fileName;
            }

        }

    }

    public enum Speciality
    {
        GENERAL_PHYSICIAN, INTERNAL_MEDICINE, CARDIOLOGY, NEUROLOGY,DERMATOLOGY, PEDIATRICS, PSYCHIATRY, ORTHOPEDICS, ENDOCRINOLOGY, GASTROENTEROLOGY, NEPHROLOGY, UROLOGY, GYNECOLOGY, OBSTETRICS, ENT, OPHTHALMOLOGY, DENTISTRY, PULMONOLOGY, ONCOLOGY, GENERAL_SURGERY, PLASTIC_SURGERY, ANESTHESIOLOGY, EMERGENCY_MEDICINE, RADIOLOGY, PATHOLOGY
    }
}
