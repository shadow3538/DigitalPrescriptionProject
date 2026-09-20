using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace DigitalPrescriptionProject.Models.ViewModels
{
    public class MedicalDocumentUploadViewModel
    {
        public int PatientId { get; set; }

        public int? PrescriptionId { get; set; }


        [Required]
        [Display(Name = "Document Type")]
        public string DocumentType { get; set; } = "";


        [Required]
        [Display(Name = "File")]
        public IFormFile? File { get; set; }
    }
}