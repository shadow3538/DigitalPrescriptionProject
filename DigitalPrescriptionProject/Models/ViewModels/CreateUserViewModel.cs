using System.ComponentModel.DataAnnotations;
using DigitalPrescriptionProject.Models;

namespace DigitalPrescriptionProject.Models.ViewModels;

public class CreateUserViewModel
{
    [Required]
    public string FirstName { get; set; } = "";

    public string LastName { get; set; } = "";

    public Speciality? Speciality { get; set; }

    public int? Age { get; set; }

    public string Phone { get; set; } = "";

    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required]
    public string Role { get; set; } = "";

    public Gender? Gender { get; set; }

    public IFormFile? Upload { get; set; }
}