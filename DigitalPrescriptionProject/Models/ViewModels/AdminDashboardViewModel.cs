namespace DigitalPrescriptionProject.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalDoctors { get; set; }
        public int ActiveDoctors { get; set; }
        public int DeletedDoctors { get; set; }

        public int TotalPatients { get; set; }
        public int ActivePatients { get; set; }
        public int DeletedPatients { get; set; }

        public int TotalPrescriptions { get; set; }
        public int ActivePrescriptions { get; set; }
        public int DeletedPrescriptions { get; set; }
    }
}