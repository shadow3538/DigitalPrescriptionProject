using DigitalPrescriptionProject.Models;

namespace DigitalPrescriptionProject.Models.ViewModels
{
    public class PatientDashboardViewModel
    {
        public string PatientName { get; set; } = "";

        public string? PatientImage { get; set; }

        public int Age { get; set; }

        public Gender Gender { get; set; }

        public string Phone { get; set; } = "";

        public string? Email { get; set; }


        public int TotalPrescriptions { get; set; }

        public int DoctorsVisited { get; set; }

        public int MedicinesPrescribed { get; set; }

        public int TestsPrescribed { get; set; }



        public DateTime? LatestVisitDate { get; set; }


        public List<Prescription> RecentPrescriptions { get; set; }
            = new();
    }
}