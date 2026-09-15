
using System.ComponentModel.DataAnnotations;

namespace DigitalPrescriptionProject.Models.ViewModels
{
    public class CreateDoctorViewModel
    {
        public string FirstName { get; set; } = "";

        public string LastName { get; set; } = "";

        public Speciality Speciality { get; set; }

        public int Age { get; set; }

        public string Phone { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        public IFormFile? Upload { get; set; }
    }
}

