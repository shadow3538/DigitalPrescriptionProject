using DigitalPrescriptionProject.Data;
using DigitalPrescriptionProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DigitalPrescriptionProject.Controllers
{
    [Authorize(Roles = "Admin,Doctor,Patient")]
    public class PrescriptionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PrescriptionController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        public async Task<IActionResult> Index()
        {
            IQueryable<Prescription> query =
                _context.Prescriptions
                .Include(p => p.PrescribedTests)
                .Include(p => p.PrescriptionItems)
                .Include(p => p.Doctor)
                .Include(p => p.Patient);


            if (User.IsInRole("Doctor"))
            {
                var doctor = await GetCurrentDoctorAsync();

                if (doctor == null)
                    return Forbid();

                query = query.Where(
                    p => p.DoctorId == doctor.DoctorId);
            }


            else if (User.IsInRole("Patient"))
            {
                var patient = await GetCurrentPatientAsync();

                if (patient == null)
                    return Forbid();

                query = query.Where(
                    p => p.PatientId == patient.PatientId);
            }


            return View(await query.ToListAsync());
        }



        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();


            var prescription =
                await _context.Prescriptions

                .Include(p => p.Patient)
                .Include(p => p.Doctor)
                .Include(p => p.PrescribedTests)
                .Include(p => p.PrescriptionItems)

                .FirstOrDefaultAsync(
                    p => p.PrescriptionId == id);


            if (prescription == null)
                return NotFound();


            if (User.IsInRole("Doctor"))
            {
                var doctor = await GetCurrentDoctorAsync();

                if (doctor == null ||
                    prescription.DoctorId != doctor.DoctorId)
                {
                    return Forbid();
                }
            }


            if (User.IsInRole("Patient"))
            {
                var patient = await GetCurrentPatientAsync();

                if (patient == null ||
                    prescription.PatientId != patient.PatientId)
                {
                    return Forbid();
                }
            }


            return View(prescription);
        }


 
        [Authorize(Roles = "Doctor,Admin")]
        public IActionResult Create()
        {
            patientDropDown();
            DoctorDropDown();

            return View(new Prescription());
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> Create(
            Prescription prescription,
            string treatmentOperation = "save",
            string testOperation = "save")
        {


            if (User.IsInRole("Doctor"))
            {
                var doctor = await GetCurrentDoctorAsync();

                if (doctor == null)
                    return Forbid();

                prescription.DoctorId =
                    doctor.DoctorId;
            }

            patientDropDown(prescription.PatientId);
            DoctorDropDown(prescription.DoctorId);


            if (treatmentOperation.Equals(
                "add",
                StringComparison.OrdinalIgnoreCase))
            {
                prescription.PrescriptionItems ??=
                    new List<PrescriptionItem>();

                prescription.PrescriptionItems.Add(
                    new PrescriptionItem());

                ModelState.Clear();

                return View(prescription);
            }


            if (treatmentOperation.StartsWith(
                "delete-",
                StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(
                    treatmentOperation.Replace(
                        "delete-", ""),
                    out int index))
                {
                    if (prescription.PrescriptionItems != null &&
                        index >= 0 &&
                        index < prescription.PrescriptionItems.Count)
                    {
                        prescription.PrescriptionItems.RemoveAt(index);
                    }
                }

                ModelState.Clear();

                return View(prescription);
            }


            if (testOperation.Equals(
                "add",
                StringComparison.OrdinalIgnoreCase))
            {
                prescription.PrescribedTests ??=
                    new List<PrescribedTest>();

                prescription.PrescribedTests.Add(
                    new PrescribedTest());

                ModelState.Clear();

                return View(prescription);
            }

            if (testOperation.StartsWith(
                "delete-",
                StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(
                    testOperation.Replace(
                        "delete-", ""),
                    out int index))
                {
                    if (prescription.PrescribedTests != null &&
                        index >= 0 &&
                        index < prescription.PrescribedTests.Count)
                    {
                        prescription.PrescribedTests.RemoveAt(index);
                    }
                }

                ModelState.Clear();

                return View(prescription);
            }


            if (ModelState.IsValid)
            {
                _context.Prescriptions.Add(
                    prescription);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }


            return View(prescription);
        }




        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();


            var prescription =
                await _context.Prescriptions

                .Include(p => p.PrescribedTests)
                .Include(p => p.PrescriptionItems)
                .Include(p => p.Doctor)
                .Include(p => p.Patient)

                .FirstOrDefaultAsync(
                    p => p.PrescriptionId == id);


            if (prescription == null)
                return NotFound();


            if (User.IsInRole("Doctor"))
            {
                var doctor =
                    await GetCurrentDoctorAsync();

                if (doctor == null ||
                    prescription.DoctorId != doctor.DoctorId)
                {
                    return Forbid();
                }
            }


            patientDropDown(
                prescription.PatientId);

            DoctorDropDown(
                prescription.DoctorId);


            return View(prescription);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> Edit(
            Prescription prescription,
            string treatmentOperation = "save",
            string testOperation = "save")
        {

            var existingPrescription =
                await _context.Prescriptions

                .Include(p => p.PrescribedTests)
                .Include(p => p.PrescriptionItems)

                .FirstOrDefaultAsync(
                    p =>
                    p.PrescriptionId ==
                    prescription.PrescriptionId);


            if (existingPrescription == null)
                return NotFound();

            if (User.IsInRole("Doctor"))
            {
                var doctor =
                    await GetCurrentDoctorAsync();

                if (doctor == null)
                    return Forbid();


                if (existingPrescription.DoctorId
                    != doctor.DoctorId)
                {
                    return Forbid();
                }


                prescription.DoctorId =
                    existingPrescription.DoctorId;

                prescription.PatientId =
                    existingPrescription.PatientId;
            }


            patientDropDown(prescription.PatientId);
            DoctorDropDown(prescription.DoctorId);

            if (treatmentOperation.Equals(
                "add",
                StringComparison.OrdinalIgnoreCase))
            {
                prescription.PrescriptionItems ??=
                    new List<PrescriptionItem>();

                prescription.PrescriptionItems.Add(
                    new PrescriptionItem());

                ModelState.Clear();

                return View(prescription);
            }

            if (treatmentOperation.StartsWith(
                "delete-",
                StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(
                    treatmentOperation.Replace(
                        "delete-", ""),
                    out int index))
                {
                    if (prescription.PrescriptionItems != null &&
                        index >= 0 &&
                        index < prescription.PrescriptionItems.Count)
                    {
                        prescription.PrescriptionItems.RemoveAt(index);
                    }
                }

                ModelState.Clear();

                return View(prescription);
            }

            if (testOperation.Equals(
                "add",
                StringComparison.OrdinalIgnoreCase))
            {
                prescription.PrescribedTests ??=
                    new List<PrescribedTest>();

                prescription.PrescribedTests.Add(
                    new PrescribedTest());

                ModelState.Clear();

                return View(prescription);
            }


            if (testOperation.StartsWith(
                "delete-",
                StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(
                    testOperation.Replace(
                        "delete-", ""),
                    out int index))
                {
                    if (prescription.PrescribedTests != null &&
                        index >= 0 &&
                        index < prescription.PrescribedTests.Count)
                    {
                        prescription.PrescribedTests.RemoveAt(index);
                    }
                }

                ModelState.Clear();

                return View(prescription);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingPrescription.VisitDate =
                        prescription.VisitDate;

                    existingPrescription.ClinicalNotes =
                        prescription.ClinicalNotes;

                    existingPrescription.Diagnosis =
                        prescription.Diagnosis;


                    if (User.IsInRole("Doctor"))
                    {
                        existingPrescription.DoctorId =
                            existingPrescription.DoctorId;

                        existingPrescription.PatientId =
                            existingPrescription.PatientId;
                    }
                    else
                    {
                        existingPrescription.DoctorId =
                            prescription.DoctorId;

                        existingPrescription.PatientId =
                            prescription.PatientId;
                    }

                    var postedTestIds =
                        (prescription.PrescribedTests ??
                         new List<PrescribedTest>())

                        .Where(t => t.Id > 0)

                        .Select(t => t.Id)

                        .ToHashSet();


                    var existingTests =
                        existingPrescription.PrescribedTests
                        ?? new List<PrescribedTest>();


                    var testsToDelete =
                        existingTests
                        .Where(t =>
                            !postedTestIds.Contains(t.Id))
                        .ToList();


                    _context.PrescribedTests
                        .RemoveRange(testsToDelete);

                    foreach (
                        var postedTest
                        in prescription.PrescribedTests
                        ?? new List<PrescribedTest>())
                    {

                        if (postedTest.Id == 0)
                        {
                            existingPrescription.PrescribedTests
                                .Add(
                                    new PrescribedTest
                                    {
                                        TestName =
                                            postedTest.TestName,

                                        Instruction =
                                            postedTest.Instruction
                                    });
                        }
                        else
                        {
                            var existingTest =
                                existingTests.FirstOrDefault(
                                    t =>
                                    t.Id ==
                                    postedTest.Id);


                            if (existingTest == null)
                                return Forbid();


                            existingTest.TestName =
                                postedTest.TestName;

                            existingTest.Instruction =
                                postedTest.Instruction;
                        }
                    }


                    var postedItemIds =
                        (prescription.PrescriptionItems ??
                         new List<PrescriptionItem>())

                        .Where(i =>
                            i.PrescriptionItemId > 0)

                        .Select(i =>
                            i.PrescriptionItemId)

                        .ToHashSet();


                    var existingItems =
                        existingPrescription
                        .PrescriptionItems
                        ?? new List<PrescriptionItem>();

                    var itemsToDelete =
                        existingItems
                        .Where(i =>
                            !postedItemIds.Contains(
                                i.PrescriptionItemId))
                        .ToList();


                    _context.PrescriptionItems
                        .RemoveRange(itemsToDelete);


                    foreach (
                        var postedItem
                        in prescription.PrescriptionItems
                        ?? new List<PrescriptionItem>())
                    {

                        if (postedItem.PrescriptionItemId == 0)
                        {
                            existingPrescription.PrescriptionItems
                                .Add(
                                    new PrescriptionItem
                                    {
                                        MedicineName =
                                            postedItem.MedicineName,

                                        Dosage =
                                            postedItem.Dosage,

                                        Duration =
                                            postedItem.Duration,

                                        Time =
                                            postedItem.Time
                                    });
                        }
                        else
                        {
                            var existingItem =
                                existingItems.FirstOrDefault(
                                    i =>
                                    i.PrescriptionItemId ==
                                    postedItem.PrescriptionItemId);


                            if (existingItem == null)
                                return Forbid();


                            existingItem.MedicineName =
                                postedItem.MedicineName;

                            existingItem.Dosage =
                                postedItem.Dosage;

                            existingItem.Duration =
                                postedItem.Duration;

                            existingItem.Time =
                                postedItem.Time;
                        }
                    }


                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PrescriptionExists(
                        prescription.PrescriptionId))
                    {
                        return NotFound();
                    }

                    throw;
                }
            }


            return View(prescription);
        }




        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();


            var prescription =
                await _context.Prescriptions

                .Include(p => p.Doctor)
                .Include(p => p.Patient)
                .Include(p => p.PrescribedTests)
                .Include(p => p.PrescriptionItems)

                .FirstOrDefaultAsync(
                    p =>
                    p.PrescriptionId == id);


            if (prescription == null)
                return NotFound();


            return View(prescription);
        }




        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(
            int? id)
        {
            if (id == null)
                return NotFound();


            var prescription =
                await _context.Prescriptions
                .FindAsync(id);


            if (prescription != null)
            {
                _context.Prescriptions
                    .Remove(prescription);

                await _context.SaveChangesAsync();
            }


            return RedirectToAction(nameof(Index));
        }




        private async Task<Doctor?>
            GetCurrentDoctorAsync()
        {
            var user =
                await _userManager.GetUserAsync(User);

            if (user == null)
                return null;


            return await _context.Doctors
                .FirstOrDefaultAsync(
                    d => d.UserId == user.Id);
        }



        private async Task<Patient?>
            GetCurrentPatientAsync()
        {
            var user =
                await _userManager.GetUserAsync(User);

            if (user == null)
                return null;


            return await _context.Patients
                .FirstOrDefaultAsync(
                    p => p.UserId == user.Id);
        }



        private void patientDropDown(
            object? select = null)
        {
            ViewBag.PatientId =
                new SelectList(
                    _context.Patients.ToList(),
                    "PatientId",
                    "FullName",
                    select);
        }



        private void DoctorDropDown(
            object? select = null)
        {
            ViewBag.DoctorId =
                new SelectList(
                    _context.Doctors.ToList(),
                    "DoctorId",
                    "FullName",
                    select);
        }



        private bool PrescriptionExists(int? id)
        {
            return _context.Prescriptions
                .Any(e =>
                    e.PrescriptionId == id);
        }
    }
}