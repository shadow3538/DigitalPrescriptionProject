using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalPrescriptionProject.Models
{
    public class TestResult
    {
        [Key]
        public int TestResultId { get; set; }


        [Required]
        public int PrescribedTestId { get; set; }


        [ForeignKey(nameof(PrescribedTestId))]
        public PrescribedTest? PrescribedTest { get; set; }


        [Required]
        [Display(Name = "Result")]
        public string ResultValue { get; set; } = "";


        [Display(Name = "Unit")]
        public string? Unit { get; set; }


        [Display(Name = "Reference Range")]
        public string? ReferenceRange { get; set; }


        [DataType(DataType.Date)]
        [Display(Name = "Report Date")]
        public DateTime ReportDate { get; set; } = DateTime.Today;


        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }
    }
}
