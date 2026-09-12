using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigitalPrescriptionProject.Models;
using DigitalPrescriptionProject.Data;
using Microsoft.AspNetCore.Identity;
using DigitalPrescriptionProject.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

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

    // GET: PATIENTS
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        if (page < 1)
            page = 1;

        if (pageSize < 1)
            pageSize = 10;

        IQueryable<Patient> query = _context.Patients;

        // Patient শুধু নিজের profile দেখতে পারবে
        if (User.IsInRole("Patient"))
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Forbid();

            query = query.Where(p => p.UserId == user.Id);
        }

        var totalPatients = await query.CountAsync();

        var patients = await query
            .OrderBy(p => p.PatientId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalPages = (int)Math.Ceiling(
            (double)totalPatients / pageSize
        );

        return View(patients);
    }


    // GET: PATIENTS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.PatientId == id);

        if (patient == null)
        {
            return NotFound();
        }

        // Patient শুধু নিজের profile দেখতে পারবে
        if (User.IsInRole("Patient"))
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || patient.UserId != user.Id)
                return Forbid();
        }

        return View(patient);
    }


    // GET: PATIENTS/Create
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View(new CreatePatientViewModel());
    }


    // POST: PATIENTS/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(
        CreatePatientViewModel model,
        [FromServices] IWebHostEnvironment env)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true
        };

        var userResult = await _userManager.CreateAsync(
            user,
            model.Password
        );

        if (!userResult.Succeeded)
        {
            foreach (var error in userResult.Errors)
            {
                ModelState.AddModelError(
                    "",
                    error.Description
                );
            }

            return View(model);
        }

        var roleResult = await _userManager.AddToRoleAsync(
            user,
            "Patient"
        );

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            foreach (var error in roleResult.Errors)
            {
                ModelState.AddModelError(
                    "",
                    error.Description
                );
            }

            return View(model);
        }

        var patient = new Patient
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Age = model.Age,
            Phone = model.Phone,
            Email = model.Email,
            Gender = model.Gender,
            UserId = user.Id,
            CreatedAt = DateTime.Now
        };

        patient.Upload = model.Upload;
        patient.SavePatientImage(env);

        _context.Patients.Add(patient);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }


    // GET: PATIENTS/Edit/5
    [Authorize(Roles = "Admin,Patient")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.PatientId == id);

        if (patient == null)
        {
            return NotFound();
        }

        // Patient শুধু নিজের profile edit করতে পারবে
        if (User.IsInRole("Patient"))
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || patient.UserId != user.Id)
                return Forbid();
        }

        return View(patient);
    }


    // POST: PATIENTS/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Patient")]
    public async Task<IActionResult> Edit(
        int? id,
        Patient patient,
        [FromServices] IWebHostEnvironment env)
    {
        if (id != patient.PatientId)
        {
            return NotFound();
        }

        // Database থেকে original patient বের করি
        var existingPatient = await _context.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.PatientId == patient.PatientId
            );

        if (existingPatient == null)
        {
            return NotFound();
        }

        // Patient অন্য Patient-এর profile modify করতে পারবে না
        if (User.IsInRole("Patient"))
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || existingPatient.UserId != user.Id)
                return Forbid();

            // UserId পরিবর্তন করতে পারবে না
            patient.UserId = existingPatient.UserId;

            // Email-ও এখানে Identity account-এর email-এর সাথে
            // manually change করার সুযোগ না দেওয়াই নিরাপদ
            patient.Email = existingPatient.Email;
        }

        if (ModelState.IsValid)
        {
            try
            {
                // নতুন image থাকলে save করবে
                patient.SavePatientImage(env);

                _context.Update(patient);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PatientExists(patient.PatientId))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        return View(patient);
    }


    // GET: PATIENTS/Delete/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.PatientId == id);

        if (patient == null)
        {
            return NotFound();
        }

        return View(patient);
    }


    // POST: PATIENTS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var patient = await _context.Patients.FindAsync(id);

        if (patient != null)
        {
            _context.Patients.Remove(patient);
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }


    private bool PatientExists(int? id)
    {
        return _context.Patients
            .Any(e => e.PatientId == id);
    }
}