using DigitalPrescriptionProject.Data;
using DigitalPrescriptionProject.Models;
using DigitalPrescriptionProject.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin,Patient")]
public class PatientController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public PatientController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }




    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> Dashboard()
    {

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Forbid();


        var patient = await _context.Patients
            .FirstOrDefaultAsync(p =>
                p.UserId == user.Id &&
                !p.IsDeleted);

        if (patient == null)
            return Forbid();



        var prescriptions =
            _context.Prescriptions
                .Where(p =>
                    p.PatientId == patient.PatientId);



        var totalPrescriptions =
            await prescriptions.CountAsync();



        var doctorsVisited =
            await prescriptions
                .Select(p => p.DoctorId)
                .Distinct()
                .CountAsync();



        var medicinesPrescribed =
            await _context.PrescriptionItems
                .Where(item =>
                    item.Prescription != null &&
                    item.Prescription.PatientId ==
                    patient.PatientId)
                .CountAsync();


        var testsPrescribed =
            await _context.PrescribedTests
                .Where(test =>
                    test.Prescription != null &&
                    test.Prescription.PatientId ==
                    patient.PatientId)
                .CountAsync();



        var latestVisitDate =
            await prescriptions
                .Select(p => (DateTime?)p.VisitDate)
                .OrderByDescending(d => d)
                .FirstOrDefaultAsync();



        var recentPrescriptions =
            await prescriptions

                .Include(p => p.Doctor)

                .OrderByDescending(
                    p => p.VisitDate)

                .ThenByDescending(
                    p => p.PrescriptionId)

                .Take(5)

                .ToListAsync();


        var model =
            new PatientDashboardViewModel
            {
                PatientName =
                    patient.FullName,

                PatientImage =
                    patient.ImagePath,

                Age =
                    patient.Age,

                Gender =
                    patient.Gender,

                Phone =
                    patient.Phone,

                Email =
                    patient.Email,

                TotalPrescriptions =
                    totalPrescriptions,

                DoctorsVisited =
                    doctorsVisited,

                MedicinesPrescribed =
                    medicinesPrescribed,

                TestsPrescribed =
                    testsPrescribed,

                LatestVisitDate =
                    latestVisitDate,

                RecentPrescriptions =
                    recentPrescriptions
            };


        return View(model);
    }






    public async Task<IActionResult> Index(
        string? search,
        int page = 1,
        int pageSize = 10)
    {
        if (page < 1)
            page = 1;

        if (pageSize < 1)
            pageSize = 10;

        IQueryable<Patient> query =
            _context.Patients
                .Where(p => !p.IsDeleted);

        if (User.IsInRole("Patient"))
        {
            var user =
                await _userManager.GetUserAsync(User);

            if (user == null)
                return Forbid();

            query = query.Where(
                p => p.UserId == user.Id);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(p =>
                p.FirstName.Contains(search) ||
                p.LastName.Contains(search) ||
                p.Phone.Contains(search) ||
                (p.Email != null &&
                 p.Email.Contains(search)));
        }

        var totalPatients =
            await query.CountAsync();

        var patients =
            await query
                .OrderBy(p => p.PatientId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

        ViewBag.Search = search;
        ViewBag.CurrentPage = page;
        ViewBag.PageSize = pageSize;

        ViewBag.TotalPages =
            (int)Math.Ceiling(
                (double)totalPatients /
                pageSize);

        return View(patients);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();


        var patient =
    await _context.Patients
        .Include(p => p.MedicalDocuments)
        .FirstOrDefaultAsync(
            p =>
                p.PatientId == id &&
                !p.IsDeleted);


        if (patient == null)
            return NotFound();

        if (User.IsInRole("Patient"))
        {
            var user =
                await _userManager.GetUserAsync(User);

            if (user == null ||
                patient.UserId != user.Id)
            {
                return Forbid();
            }
        }

        return View(patient);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View(
            new CreatePatientViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(
        CreatePatientViewModel model,
        [FromServices] IWebHostEnvironment env)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true,
            MustChangePassword = true,
            PhoneNumber = model.Phone
        };

        var userResult =
            await _userManager.CreateAsync(
                user,
                model.Password);

        if (!userResult.Succeeded)
        {
            foreach (var error in userResult.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return View(model);
        }

        var roleResult =
            await _userManager.AddToRoleAsync(
                user,
                "Patient");

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            foreach (var error in roleResult.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return View(model);
        }

        var patient = new Patient
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Age = model.Age,
            Gender = model.Gender,
            Phone = model.Phone,
            Email = model.Email,
            UserId = user.Id,
            CreatedAt = DateTime.Now,
            IsDeleted = false,
            DeletedAt = null
        };

        patient.Upload = model.Upload;

        try
        {
            patient.SavePatientImage(env);
        }
        catch (Exception ex)
        {
            await _userManager.DeleteAsync(user);

            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            return View(model);
        }

        _context.Patients.Add(patient);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch
        {
            await _userManager.DeleteAsync(user);
            throw;
        }

        return RedirectToAction(
            nameof(Index));
    }

    [Authorize(Roles = "Admin,Patient")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var patient =
            await _context.Patients
                .FirstOrDefaultAsync(
                    p =>
                        p.PatientId == id &&
                        !p.IsDeleted);

        if (patient == null)
            return NotFound();

        if (User.IsInRole("Patient"))
        {
            var user =
                await _userManager.GetUserAsync(User);

            if (user == null ||
                patient.UserId != user.Id)
            {
                return Forbid();
            }
        }

        return View(patient);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Patient")]
    public async Task<IActionResult> Edit(
        int? id,
        Patient patient,
        [FromServices] IWebHostEnvironment env)
    {
        if (id != patient.PatientId)
            return NotFound();

        var existingPatient =
            await _context.Patients
                .FirstOrDefaultAsync(
                    p =>
                        p.PatientId ==
                        patient.PatientId &&
                        !p.IsDeleted);

        if (existingPatient == null)
            return NotFound();

        if (User.IsInRole("Patient"))
        {
            var user =
                await _userManager.GetUserAsync(User);

            if (user == null ||
                existingPatient.UserId != user.Id)
            {
                return Forbid();
            }
        }

        if (!ModelState.IsValid)
            return View(patient);

        try
        {
            existingPatient.FirstName =
                patient.FirstName;

            existingPatient.LastName =
                patient.LastName;

            existingPatient.Age =
                patient.Age;

            existingPatient.Phone =
                patient.Phone;

            existingPatient.Gender =
                patient.Gender;

            if (patient.Upload != null)
            {
                existingPatient.Upload =
                    patient.Upload;

                existingPatient.SavePatientImage(env);
            }

            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PatientExists(
                patient.PatientId))
            {
                return NotFound();
            }

            throw;
        }

        return RedirectToAction(
            nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var patient =
            await _context.Patients
                .FirstOrDefaultAsync(
                    p =>
                        p.PatientId == id &&
                        !p.IsDeleted);

        if (patient == null)
            return NotFound();

        return View(patient);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        if (id == null)
            return NotFound();


        var patient =
            await _context.Patients
                .FirstOrDefaultAsync(
                    p =>
                        p.PatientId == id &&
                        !p.IsDeleted);


        if (patient == null)
            return NotFound();


        var hasPrescription =
            await _context.Prescriptions
                .AnyAsync(p =>
                    p.PatientId == patient.PatientId);


        if (hasPrescription)
        {
            patient.IsDeleted = true;

            patient.DeletedAt = DateTime.Now;


            if (!string.IsNullOrEmpty(patient.UserId))
            {
                var user =
                    await _userManager.FindByIdAsync(
                        patient.UserId);

                if (user != null)
                {
                    user.IsActive = false;
                }
            }
        }


        else
        {

            _context.Patients.Remove(patient);


            if (!string.IsNullOrEmpty(patient.UserId))
            {
                var user =
                    await _userManager.FindByIdAsync(
                        patient.UserId);

                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }
            }
        }


        await _context.SaveChangesAsync();


        return RedirectToAction(
            nameof(Index));
    }





    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Restore(int? id)
    {
        if (id == null)
            return NotFound();

        var patient =
            await _context.Patients
                .FirstOrDefaultAsync(
                    p =>
                        p.PatientId == id &&
                        p.IsDeleted);

        if (patient == null)
            return NotFound();

        return View(patient);
    }

    [HttpPost]
    [ActionName("Restore")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RestoreConfirmed(
        int? id)
    {
        if (id == null)
            return NotFound();

        var patient =
            await _context.Patients
                .FirstOrDefaultAsync(
                    p =>
                        p.PatientId == id &&
                        p.IsDeleted);

        if (patient == null)
            return NotFound();

        patient.IsDeleted = false;
        patient.DeletedAt = null;

        if (!string.IsNullOrEmpty(patient.UserId))
        {
            var user =
                await _userManager.FindByIdAsync(
                    patient.UserId);

            if (user != null)
            {
                user.IsActive = true;
            }
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(
            nameof(Deleted));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deleted()
    {
        var deletedPatients =
            await _context.Patients
                .Where(p => p.IsDeleted)
                .OrderByDescending(
                    p => p.DeletedAt)
                .ToListAsync();

        return View(deletedPatients);
    }

    private bool PatientExists(int? id)
    {
        return _context.Patients
            .Any(
                p =>
                    p.PatientId == id &&
                    !p.IsDeleted);
    }
}