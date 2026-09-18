using System.ComponentModel.DataAnnotations;

namespace DigitalPrescriptionProject.Models.ViewModels
{
    public class CreateDoctorViewModel
    {
        [Required]
        public string FirstName { get; set; } = "";

        [Required]
        public string LastName { get; set; } = "";

        [Required]
        public Speciality Speciality { get; set; }

        [Required]
        [Range(25, 100)]
        public int Age { get; set; }

        [Required]
        public string Phone { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        public IFormFile? Upload { get; set; }
    }
}