using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalPrescriptionProject.Models
{
    public class Prescription
    {
        [Key]
        public int PrescriptionId { get; set; }

        [NotMapped]
        public string PrescriptionNumber =>
            $"RX-{PrescriptionId:D6}";

        [DataType(DataType.Date)]
        [DisplayFormat(
            ApplyFormatInEditMode = true,
            DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime VisitDate { get; set; } = DateTime.Today;

        [Required]
        public string ClinicalNotes { get; set; } = "";

        [Required]
        public string Diagnosis { get; set; } = "";

        [ForeignKey("Patient")]
        public int PatientId { get; set; }

        public Patient? Patient { get; set; }

        public int DoctorId { get; set; }

        public Doctor? Doctor { get; set; }

        public List<PrescriptionItem> PrescriptionItems { get; set; } = new();

        public List<PrescribedTest> PrescribedTests { get; set; } = new();
        public List<MedicalDocument> MedicalDocuments { get; set; } = new();
    }
}