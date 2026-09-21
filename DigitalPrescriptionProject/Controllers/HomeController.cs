using DigitalPrescriptionProject.Data;
using DigitalPrescriptionProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalPrescriptionProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }



        public async Task<IActionResult> Index()
        {

            ViewBag.DoctorCount = await _context.Doctors.CountAsync();

            ViewBag.PatientCount = await _context.Patients.CountAsync();

            ViewBag.PrescriptionCount = await _context.Prescriptions.CountAsync();


            ViewBag.RecentDoctors = await _context.Doctors
                    .OrderByDescending(d => d.DoctorId)
                    .Take(5)
                    .ToListAsync();


            ViewBag.RecentPatients = await _context.Patients
                    .OrderByDescending(p => p.PatientId)
                    .Take(5)
                    .ToListAsync();


            return View();
        }




        public IActionResult Create()
        {
            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }
    }
}