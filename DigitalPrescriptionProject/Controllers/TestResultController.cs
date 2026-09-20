using DigitalPrescriptionProject.Data;
using DigitalPrescriptionProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalPrescriptionProject.Controllers
{
    [Authorize(Roles = "Admin,Doctor")]
    public class TestResultController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TestResultController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }



        [HttpGet]
        public async Task<IActionResult> Create(
            int prescribedTestId)
        {
            var prescribedTest =
                await _context.PrescribedTests

                    .Include(t => t.Prescription)

                    .ThenInclude(p => p!.Patient)

                    .FirstOrDefaultAsync(
                        t => t.Id == prescribedTestId);


            if (prescribedTest == null)
                return NotFound();


            if (!await CanManagePrescriptionAsync(
                prescribedTest.PrescriptionId))
            {
                return Forbid();
            }


            if (await _context.TestResults
                .AnyAsync(r =>
                    r.PrescribedTestId ==
                    prescribedTestId))
            {
                var existingResult =
                    await _context.TestResults
                        .FirstAsync(r =>
                            r.PrescribedTestId ==
                            prescribedTestId);

                return RedirectToAction(
                    nameof(Edit),
                    new
                    {
                        id = existingResult.TestResultId
                    });
            }


            var result = new TestResult
            {
                PrescribedTestId =
                    prescribedTest.Id,

                PrescribedTest =
                    prescribedTest,

                ReportDate =
                    DateTime.Today
            };


            return View(result);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            TestResult model)
        {
            var prescribedTest =
                await _context.PrescribedTests

                    .Include(t => t.Prescription)

                    .FirstOrDefaultAsync(
                        t => t.Id ==
                             model.PrescribedTestId);


            if (prescribedTest == null)
                return NotFound();


            if (!await CanManagePrescriptionAsync(
                prescribedTest.PrescriptionId))
            {
                return Forbid();
            }


            if (await _context.TestResults
                .AnyAsync(r =>
                    r.PrescribedTestId ==
                    model.PrescribedTestId))
            {
                ModelState.AddModelError(
                    "",
                    "A result already exists for this test.");

                model.PrescribedTest =
                    prescribedTest;

                return View(model);
            }


            if (!ModelState.IsValid)
            {
                model.PrescribedTest =
                    prescribedTest;

                return View(model);
            }


            var result = new TestResult
            {
                PrescribedTestId =
                    prescribedTest.Id,

                ResultValue =
                    model.ResultValue,

                Unit =
                    model.Unit,

                ReferenceRange =
                    model.ReferenceRange,

                ReportDate =
                    model.ReportDate,

                Remarks =
                    model.Remarks
            };


            _context.TestResults.Add(result);

            await _context.SaveChangesAsync();


            return RedirectToAction(
                "Details",
                "Prescription",
                new
                {
                    id =
                        prescribedTest.PrescriptionId
                });
        }



        [HttpGet]
        public async Task<IActionResult> Edit(
            int? id)
        {
            if (id == null)
                return NotFound();


            var result =
                await _context.TestResults

                    .Include(r => r.PrescribedTest)

                    .ThenInclude(t =>
                        t!.Prescription)

                    .FirstOrDefaultAsync(
                        r =>
                        r.TestResultId == id);


            if (result == null)
                return NotFound();


            if (result.PrescribedTest == null)
                return NotFound();


            if (!await CanManagePrescriptionAsync(
                result.PrescribedTest.PrescriptionId))
            {
                return Forbid();
            }


            return View(result);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            TestResult model)
        {
            var result =
                await _context.TestResults

                    .Include(r => r.PrescribedTest)

                    .ThenInclude(t =>
                        t!.Prescription)

                    .FirstOrDefaultAsync(
                        r =>
                        r.TestResultId == id);


            if (result == null)
                return NotFound();


            if (result.PrescribedTest == null)
                return NotFound();


            if (!await CanManagePrescriptionAsync(
                result.PrescribedTest.PrescriptionId))
            {
                return Forbid();
            }


            if (!ModelState.IsValid)
            {
                model.PrescribedTest =
                    result.PrescribedTest;

                model.TestResultId =
                    result.TestResultId;

                model.PrescribedTestId =
                    result.PrescribedTestId;

                return View(model);
            }


            result.ResultValue =
                model.ResultValue;

            result.Unit =
                model.Unit;

            result.ReferenceRange =
                model.ReferenceRange;

            result.ReportDate =
                model.ReportDate;

            result.Remarks =
                model.Remarks;


            await _context.SaveChangesAsync();


            return RedirectToAction(
                "Details",
                "Prescription",
                new
                {
                    id =
                        result.PrescribedTest
                            .PrescriptionId
                });
        }




        private async Task<bool>
            CanManagePrescriptionAsync(
                int prescriptionId)
        {
            var prescription =
                await _context.Prescriptions
                    .FirstOrDefaultAsync(
                        p =>
                        p.PrescriptionId ==
                        prescriptionId);


            if (prescription == null)
                return false;


            if (User.IsInRole("Admin"))
                return true;


            if (User.IsInRole("Doctor"))
            {
                var user =
                    await _userManager
                        .GetUserAsync(User);

                if (user == null)
                    return false;


                var doctor =
                    await _context.Doctors
                        .FirstOrDefaultAsync(
                            d =>
                            d.UserId == user.Id &&
                            !d.IsDeleted);


                if (doctor == null)
                    return false;


                return prescription.DoctorId ==
                       doctor.DoctorId;
            }


            return false;
        }
    }
}