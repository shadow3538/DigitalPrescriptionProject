using DigitalPrescriptionProject.Data;
using DigitalPrescriptionProject.Models;
using DigitalPrescriptionProject.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalPrescriptionProject.Controllers
{
    [Authorize(Roles = "Admin,Doctor,Patient")]
    public class MedicalDocumentController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IWebHostEnvironment _environment;


        public MedicalDocumentController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }




        [HttpGet]
        public async Task<IActionResult> Upload(
            int patientId,
            int? prescriptionId)
        {
            var hasAccess =
                await CanAccessPatientAsync(
                    patientId,
                    prescriptionId);

            if (!hasAccess)
                return Forbid();


            var patient =
                await _context.Patients
                    .FirstOrDefaultAsync(
                        p =>
                            p.PatientId == patientId &&
                            !p.IsDeleted);

            if (patient == null)
                return NotFound();


            var model =
                new MedicalDocumentUploadViewModel
                {
                    PatientId = patientId,

                    PrescriptionId =
                        prescriptionId
                };


            ViewBag.PatientName =
                patient.FullName;


            if (prescriptionId.HasValue)
            {
                var prescription =
                    await _context.Prescriptions
                        .FirstOrDefaultAsync(
                            p =>
                                p.PrescriptionId ==
                                prescriptionId.Value);

                if (prescription == null)
                    return NotFound();


                ViewBag.PrescriptionNumber =
                    prescription
                        .PrescriptionNumber;
            }


            return View(model);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(
            MedicalDocumentUploadViewModel model)
        {
            var hasAccess =
                await CanAccessPatientAsync(
                    model.PatientId,
                    model.PrescriptionId);

            if (!hasAccess)
                return Forbid();


            if (model.File == null ||
                model.File.Length == 0)
            {
                ModelState.AddModelError(
                    "File",
                    "Please select a file.");
            }


            if (model.File != null &&
                model.File.Length >
                10 * 1024 * 1024)
            {
                ModelState.AddModelError(
                    "File",
                    "File size cannot exceed 10 MB.");
            }


            var allowedExtensions =
                new[]
                {
                    ".pdf",
                    ".jpg",
                    ".jpeg",
                    ".png",
                    ".webp"
                };


            string extension = "";


            if (model.File != null)
            {
                extension =
                    Path.GetExtension(
                        model.File.FileName)
                    .ToLowerInvariant();


                if (!allowedExtensions
                    .Contains(extension))
                {
                    ModelState.AddModelError(
                        "File",
                        "Only PDF, JPG, JPEG, PNG and WEBP files are allowed.");
                }
            }


            if (!ModelState.IsValid)
            {
                var patient =
                    await _context.Patients
                        .FirstOrDefaultAsync(
                            p =>
                                p.PatientId ==
                                model.PatientId);

                ViewBag.PatientName =
                    patient?.FullName ??
                    "N/A";


                if (model.PrescriptionId.HasValue)
                {
                    var prescription =
                        await _context.Prescriptions
                            .FirstOrDefaultAsync(
                                p =>
                                    p.PrescriptionId ==
                                    model.PrescriptionId.Value);

                    ViewBag.PrescriptionNumber =
                        prescription
                            ?.PrescriptionNumber;
                }


                return View(model);
            }




            var storageFolder =
                Path.Combine(
                    _environment.ContentRootPath,
                    "MedicalDocuments");


            Directory.CreateDirectory(
                storageFolder);




            var storedFileName =
                $"{Guid.NewGuid():N}{extension}";


            var physicalPath =
                Path.Combine(
                    storageFolder,
                    storedFileName);


            await using (
                var stream =
                    new FileStream(
                        physicalPath,
                        FileMode.Create))
            {
                await model.File!
                    .CopyToAsync(stream);
            }



            var user =
                await _userManager
                    .GetUserAsync(User);



            var document =
                new MedicalDocument
                {
                    PatientId =
                        model.PatientId,

                    PrescriptionId =
                        model.PrescriptionId,

                    DocumentType =
                        model.DocumentType.Trim(),

                    OriginalFileName =
                        Path.GetFileName(
                            model.File!.FileName),

                    StoredFileName =
                        storedFileName,

                    FilePath =
                        physicalPath,

                    UploadedByUserId =
                        user?.Id,

                    UploadedAt =
                        DateTime.Now
                };


            _context.MedicalDocuments.Add(
                document);


            await _context.SaveChangesAsync();



            if (model.PrescriptionId.HasValue)
            {
                return RedirectToAction(
                    "Details",
                    "Prescription",
                    new
                    {
                        id =
                            model.PrescriptionId.Value
                    });
            }


            return RedirectToAction(
                "Details",
                "Patient",
                new
                {
                    id =
                        model.PatientId
                });
        }




        [HttpGet]
        public async Task<IActionResult> Download(
            int id)
        {
            var document =
                await _context.MedicalDocuments
                    .Include(d => d.Prescription)
                    .FirstOrDefaultAsync(
                        d =>
                            d.MedicalDocumentId ==
                            id);


            if (document == null)
                return NotFound();


            var hasAccess =
                await CanAccessPatientAsync(
                    document.PatientId,
                    document.PrescriptionId);


            if (!hasAccess)
                return Forbid();


            if (!System.IO.File.Exists(
                document.FilePath))
            {
                return NotFound(
                    "The file could not be found.");
            }


            var extension =
                Path.GetExtension(
                    document.OriginalFileName)
                .ToLowerInvariant();


            var contentType =
                extension switch
                {
                    ".pdf" =>
                        "application/pdf",

                    ".jpg" =>
                        "image/jpeg",

                    ".jpeg" =>
                        "image/jpeg",

                    ".png" =>
                        "image/png",

                    ".webp" =>
                        "image/webp",

                    _ =>
                        "application/octet-stream"
                };


            var fileBytes =
                await System.IO.File.ReadAllBytesAsync(
                    document.FilePath);


            return File(
                fileBytes,
                contentType,
                document.OriginalFileName);
        }




        private async Task<bool>
            CanAccessPatientAsync(
                int patientId,
                int? prescriptionId)
        {

            if (User.IsInRole("Admin"))
                return await _context.Patients
                    .AnyAsync(
                        p =>
                            p.PatientId ==
                            patientId &&
                            !p.IsDeleted);



            if (User.IsInRole("Patient"))
            {
                var user =
                    await _userManager
                        .GetUserAsync(User);


                if (user == null)
                    return false;


                return await _context.Patients
                    .AnyAsync(
                        p =>
                            p.PatientId ==
                            patientId &&

                            p.UserId == user.Id &&

                            !p.IsDeleted);
            }



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


                if (!prescriptionId.HasValue)
                    return false;


                return await _context.Prescriptions
                    .AnyAsync(
                        p =>
                            p.PrescriptionId ==
                            prescriptionId.Value &&

                            p.PatientId ==
                            patientId &&

                            p.DoctorId ==
                            doctor.DoctorId);
            }


            return false;
        }
    }
}