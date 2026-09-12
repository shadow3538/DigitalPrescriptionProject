using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalPrescriptionProject.Models
{
    public class PrescriptionItem
    {
        [Key]
        public int PrescriptionItemId { get; set; }

        [DisplayName("Medicine Name")]
        public string MedicineName { get; set; } = default!;

        public string Dosage { get; set; }= default!;

        public string Duration { get; set; } = default!;
        public string Time { get; set; } = "";

        [ForeignKey("Prescription")]
        public int PrescriptionId { get; set; }
        public Prescription? Prescription { get; set; }
    }

}
