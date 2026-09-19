using DigitalPrescriptionProject.Data;
using DigitalPrescriptionProject.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalPrescriptionProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }



        // GET: Admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {

            var totalDoctors = await _context.Doctors
                .CountAsync(d => !d.IsDeleted);

            var deletedDoctors = await _context.Doctors
                .CountAsync(d => d.IsDeleted);

            var totalPatients = await _context.Patients
                .CountAsync(p => !p.IsDeleted);

            var deletedPatients = await _context.Patients
                .CountAsync(p => p.IsDeleted);

            var totalPrescriptions =
                await _context.Prescriptions.CountAsync();


            ViewBag.TotalDoctors = totalDoctors;
            ViewBag.DeletedDoctors = deletedDoctors;

            ViewBag.TotalPatients = totalPatients;
            ViewBag.DeletedPatients = deletedPatients;

            ViewBag.TotalPrescriptions = totalPrescriptions;


            return View();
        }


        // GET: Admin/Create
        public IActionResult Create()
        {
            return View();
        }


        // GET: Admin/Restore
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Restore()
        {
            var deletedDoctors = await _context.Doctors
                .Where(d => d.IsDeleted)
                .ToListAsync();

            var deletedPatients = await _context.Patients
                .Where(p => p.IsDeleted)
                .ToListAsync();

            ViewBag.DeletedDoctors = deletedDoctors;
            ViewBag.DeletedPatients = deletedPatients;

            return View();
        }


        // POST: Admin/RestoreDoctor
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RestoreDoctor(int id)
        {
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.DoctorId == id);

            if (doctor == null)
                return NotFound();

            doctor.IsDeleted = false;
            doctor.DeletedAt = null;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Restore));
        }


        // POST: Admin/RestorePatient
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RestorePatient(int id)
        {
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.PatientId == id);

            if (patient == null)
                return NotFound();

            patient.IsDeleted = false;
            patient.DeletedAt = null;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Restore));
        }
    }
}