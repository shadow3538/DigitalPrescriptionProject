using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalPrescriptionProject.Models
{
    public class Patient
    {
        [Key]
        public int PatientId { get; set; }
        public string? UserId { get; set; }

        [Required]
        [DisplayName("First Name")]
        public string FirstName { get; set; } = default!;

        [Required]
        [Display(Name ="Last Name")]
        public string LastName { get; set; } = default!;

        [NotMapped]
        [DisplayName("Name")]
        public string FullName => FirstName + " " + LastName;

        [Range(0,150)]
        public int Age { get; set; }


        public string Phone { get; set; }= default!;

        [EmailAddress]
        public string? Email { get; set; }

        [Display(Name ="Image")]
        public string? ImagePath { get; set; }

        [NotMapped]
        public IFormFile? Upload { get; set; }

        //[NotMapped]
        //public string Operation { get; set; } = "save";

        public Gender Gender { get; set; }

        [DataType(DataType.Date)]

        [DisplayFormat(ApplyFormatInEditMode =true, DataFormatString ="{0:yyyy-MM-dd}")]
        public DateTime CreatedAt { get; set; } = DateTime.Today;

        public List<Prescription> Prescriptions { get; set; } = new();

        public void SavePatientImage(IWebHostEnvironment env)
        {
            if(Upload is not null)
            {
                var fileName = $"/Images/PatientsImage/_{Guid.NewGuid()}_{Upload.FileName}";
                using Stream stream = File.Create(env.WebRootPath + fileName);
                Upload.CopyTo(stream);
                ImagePath = fileName;
            }

        }
    }

    public enum Gender
    {
        Male, Female
    }
}
