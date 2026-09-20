using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalPrescriptionProject.Models
{
    public class MedicalDocument
    {
        [Key]
        public int MedicalDocumentId { get; set; }


        [Required]
        public int PatientId { get; set; }

        public Patient? Patient { get; set; }


        public int? PrescriptionId { get; set; }

        public Prescription? Prescription { get; set; }


        [Required]
        [Display(Name = "Document Type")]
        public string DocumentType { get; set; } = "";


        [Required]
        public string OriginalFileName { get; set; } = "";


        [Required]
        public string StoredFileName { get; set; } = "";


        [Required]
        public string FilePath { get; set; } = "";


        public string? UploadedByUserId { get; set; }


        public DateTime UploadedAt { get; set; }
            = DateTime.Now;
    }
}