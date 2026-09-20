using DigitalPrescriptionProject.Models;

namespace DigitalPrescriptionProject.Models.ViewModels
{
    public class DoctorDashboardViewModel
    {
        public string DoctorName { get; set; } = "";

        public string? DoctorImage { get; set; }

        public string Speciality { get; set; } = "";

        public int TotalPatients { get; set; }

        public int TotalPrescriptions { get; set; }

        public int TodayPrescriptions { get; set; }

        public List<Prescription> RecentPrescriptions { get; set; }
            = new();
    }
}