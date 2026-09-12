using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalPrescriptionProject.Models
{
    public class PrescribedTest
    {
        [Key]
        public int Id { get; set; }


        [Display(Name ="Test Name")]
        public TestName TestName { get; set; }
        public string? Instruction { get; set; } = "N/A";

        [ForeignKey("Prescription")]
        public int PrescriptionId { get; set; }
        public Prescription? Prescription { get; set; }
    }
    public enum TestName
    {
        CBC, ESR, CRP, BLOOD_GLUCOSE, FASTING_BLOOD_SUGAR, HBA1C, LIPID_PROFILE, LIVER_FUNCTION_TEST, KIDNEY_FUNCTION_TEST, THYROID_FUNCTION_TEST, URINE_ROUTINE, URINE_CULTURE, STOOL_ROUTINE, STOOL_CULTURE, BLOOD_GROUP, SERUM_CREATININE, BLOOD_UREA, SERUM_ELECTROLYTES, SERUM_URIC_ACID, BILIRUBIN, SGPT_ALT, SGOT_AST, ALKALINE_PHOSPHATASE, SERUM_ALBUMIN, CALCIUM, VITAMIN_D, VITAMIN_B12, IRON_PROFILE, FERRITIN, TROPONIN_I, TROPONIN_T, D_DIMER, PT, INR, APTT, HEPATITIS_B_SURFACE_ANTIGEN, ANTI_HCV, HIV_TEST, VDRL, WIDAL_TEST, DENGUE_NS1, DENGUE_IGG_IGM, MALARIA_TEST, COVID_19_PCR, COVID_19_ANTIGEN, BLOOD_CULTURE, SEMEN_ANALYSIS, PAP_SMEAR, PREGNANCY_TEST, PSA, CA_125, CA_19_9, CEA, AFP, URIC_ACID, ECG, ECHOCARDIOGRAM, X_RAY, ULTRASOUND, CT_SCAN, MRI, MAMMOGRAPHY, ENDOSCOPY, COLONOSCOPY

    }
}
