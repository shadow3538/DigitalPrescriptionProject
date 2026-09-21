using DigitalPrescriptionProject.Data;
using DigitalPrescriptionProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace DigitalPrescriptionProject.Controllers
{
    [Authorize(Roles = "Admin,Doctor,Patient")]
    public class PrescriptionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDistributedCache _cache;

        public PrescriptionController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IDistributedCache cache)
        {
            _context = context;
            _userManager = userManager;
            _cache = cache;
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

        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> FollowUp(int patientId)
        {
            var patient =
                await _context.Patients
                    .FirstOrDefaultAsync(
                        p => p.PatientId == patientId &&
                             !p.IsDeleted);

            if (patient == null)
                return NotFound();

            if (User.IsInRole("Patient"))
            {
                var currentPatient =
                    await GetCurrentPatientAsync();

                if (currentPatient == null ||
                    currentPatient.PatientId != patientId)
                {
                    return Forbid();
                }
            }

            var prescriptions =
                await _context.Prescriptions
                    .Include(p => p.Doctor)
                    .Include(p => p.PrescriptionItems)
                    .Include(p => p.PrescribedTests)
                    .Where(p => p.PatientId == patientId)
                    .OrderByDescending(p => p.VisitDate)
                    .ToListAsync();

            ViewBag.PatientName = patient.FullName;
            ViewBag.PatientId = patient.PatientId;

            return View(prescriptions);
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
                    .ThenInclude(t => t.TestResult)
                .Include(p => p.PrescriptionItems)
                .Include(p => p.MedicalDocuments)
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
        public async Task<IActionResult> Create()
        {
            await patientDropDown();

            if (User.IsInRole("Admin"))
            {
                await DoctorDropDown();
            }

            return View(new Prescription());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> Create(
            Prescription prescription,
            [FromServices] IWebHostEnvironment env,
            string treatmentOperation = "save",
            string testOperation = "save")
        {
            if (User.IsInRole("Doctor"))
            {
                var doctor =
                    await GetCurrentDoctorAsync();

                if (doctor == null)
                    return Forbid();

                prescription.DoctorId =
                    doctor.DoctorId;
            }

            await patientDropDown(
                prescription.PatientId);

            if (User.IsInRole("Admin"))
            {
                await DoctorDropDown(
                    prescription.DoctorId);
            }

            if (treatmentOperation.Equals(
                "add",
                StringComparison.OrdinalIgnoreCase))
            {
                prescription.PrescriptionItems ??=
                    new List<PrescriptionItem>();

                prescription.PrescriptionItems.Add(
                    new PrescriptionItem());

                ModelState.Clear();

                if (IsAjaxRequest())
                {
                    return PartialView(
                        "_PrescriptionCollections",
                        prescription);
                }

                return View(prescription);
            }

            if (treatmentOperation.StartsWith(
                "delete-",
                StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(
                    treatmentOperation.Replace(
                        "delete-",
                        ""),
                    out int index))
                {
                    if (prescription.PrescriptionItems != null &&
                        index >= 0 &&
                        index < prescription.PrescriptionItems.Count)
                    {
                        prescription.PrescriptionItems.RemoveAt(
                            index);
                    }
                }

                ModelState.Clear();

                if (IsAjaxRequest())
                {
                    return PartialView(
                        "_PrescriptionCollections",
                        prescription);
                }

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

                if (IsAjaxRequest())
                {
                    return PartialView(
                        "_PrescriptionCollections",
                        prescription);
                }

                return View(prescription);
            }

            if (testOperation.StartsWith(
                "delete-",
                StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(
                    testOperation.Replace(
                        "delete-",
                        ""),
                    out int index))
                {
                    if (prescription.PrescribedTests != null &&
                        index >= 0 &&
                        index < prescription.PrescribedTests.Count)
                    {
                        prescription.PrescribedTests.RemoveAt(
                            index);
                    }
                }

                ModelState.Clear();

                if (IsAjaxRequest())
                {
                    return PartialView(
                        "_PrescriptionCollections",
                        prescription);
                }

                return View(prescription);
            }

            if (ModelState.IsValid)
            {
                _context.Prescriptions.Add(
                    prescription);

                await _context.SaveChangesAsync();

                return RedirectToAction(
                    nameof(Index));
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

            await patientDropDown(
                prescription.PatientId);

            await DoctorDropDown(
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

            await patientDropDown(
                prescription.PatientId);

            await DoctorDropDown(
                prescription.DoctorId);

            if (treatmentOperation.Equals(
                "add",
                StringComparison.OrdinalIgnoreCase))
            {
                prescription.PrescriptionItems ??=
                    new List<PrescriptionItem>();

                prescription.PrescriptionItems.Add(
                    new PrescriptionItem());

                ModelState.Clear();

                if (IsAjaxRequest())
                {
                    return PartialView(
                        "_PrescriptionCollections",
                        prescription);
                }

                return View(prescription);
            }

            if (treatmentOperation.StartsWith(
                "delete-",
                StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(
                    treatmentOperation.Replace(
                        "delete-",
                        ""),
                    out int index))
                {
                    if (prescription.PrescriptionItems != null &&
                        index >= 0 &&
                        index < prescription.PrescriptionItems.Count)
                    {
                        prescription.PrescriptionItems.RemoveAt(
                            index);
                    }
                }

                ModelState.Clear();

                if (IsAjaxRequest())
                {
                    return PartialView(
                        "_PrescriptionCollections",
                        prescription);
                }

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

                if (IsAjaxRequest())
                {
                    return PartialView(
                        "_PrescriptionCollections",
                        prescription);
                }

                return View(prescription);
            }

            if (testOperation.StartsWith(
                "delete-",
                StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(
                    testOperation.Replace(
                        "delete-",
                        ""),
                    out int index))
                {
                    if (prescription.PrescribedTests != null &&
                        index >= 0 &&
                        index < prescription.PrescribedTests.Count)
                    {
                        prescription.PrescribedTests.RemoveAt(
                            index);
                    }
                }

                ModelState.Clear();

                if (IsAjaxRequest())
                {
                    return PartialView(
                        "_PrescriptionCollections",
                        prescription);
                }

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
                                existingTests
                                    .FirstOrDefault(
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
                                existingItems
                                    .FirstOrDefault(
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

                    return RedirectToAction(
                        nameof(Index));
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




        private async Task patientDropDown(
            object? select = null)
        {
            const string cacheKey =
                "patients:dropdown";

            var cachedData =
                await _cache.GetStringAsync(cacheKey);

            List<PatientDropdownItem>? patients = null;

            if (!string.IsNullOrEmpty(cachedData))
            {
                patients =
                    JsonSerializer.Deserialize<
                        List<PatientDropdownItem>>(
                            cachedData);
            }

            if (patients == null)
            {
                patients =
                    await _context.Patients
                        .Where(p => !p.IsDeleted)
                        .Select(p =>
                            new PatientDropdownItem
                            {
                                PatientId =
                                    p.PatientId,

                                FullName =
                                    p.FullName
                            })
                        .ToListAsync();

                var json =
                    JsonSerializer.Serialize(patients);

                await _cache.SetStringAsync(
                    cacheKey,
                    json,
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow =
                            TimeSpan.FromMinutes(5)
                    });
            }

            ViewBag.PatientId =
                new SelectList(
                    patients,
                    "PatientId",
                    "FullName",
                    select);
        }

        private async Task DoctorDropDown(
            object? select = null)
        {
            ViewBag.DoctorId =
                new SelectList(
                    await _context.Doctors.ToListAsync(),
                    "DoctorId",
                    "FullName",
                    select);
        }




        [HttpGet]
        [Authorize(Roles = "Doctor,Admin")]
        public IActionResult LoadPrescriptionCollections()
        {
            var prescription = new Prescription
            {
                PrescriptionItems =
                    new List<PrescriptionItem>(),

                PrescribedTests =
                    new List<PrescribedTest>()
            };

            return PartialView(
                "_PrescriptionCollections",
                prescription);
        }

        private bool IsAjaxRequest()
        {
            return Request.Headers["X-Requested-With"]
                == "XMLHttpRequest";
        }

        private bool PrescriptionExists(int? id)
        {
            return _context.Prescriptions
                .Any(e =>
                    e.PrescriptionId == id);
        }

        private class PatientDropdownItem
        {
            public int PatientId { get; set; }
            public string FullName { get; set; } = "";
        }
    }
}