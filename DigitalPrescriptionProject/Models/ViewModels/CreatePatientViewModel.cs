using System.ComponentModel.DataAnnotations;

namespace DigitalPrescriptionProject.Models.ViewModels
{
    public class CreatePatientViewModel
    {
        public string FirstName { get; set; } = "";

        public string LastName { get; set; } = "";

        public int Age { get; set; }

        public string Phone { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        public Gender Gender { get; set; }

        public IFormFile? Upload { get; set; }
    }
}
