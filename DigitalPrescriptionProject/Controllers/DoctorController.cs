using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigitalPrescriptionProject.Models;
using DigitalPrescriptionProject.Data;
using Microsoft.AspNetCore.Identity;
using DigitalPrescriptionProject.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;



[Authorize(Roles = "Admin,Doctor")]
public class DoctorController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DoctorController( ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }




    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> Dashboard()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Forbid();


        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d =>
                d.UserId == user.Id &&
                !d.IsDeleted);


        if (doctor == null)
            return Forbid();



        var prescriptions = _context.Prescriptions
                .Where(p => p.DoctorId == doctor.DoctorId);


        var totalPrescriptions = await prescriptions.CountAsync();


        var totalPatients = await prescriptions
                .Select(p => p.PatientId)
                .Distinct()
                .CountAsync();


        var today = DateTime.Today;

        var tomorrow = today.AddDays(1);

        var todayPrescriptions = await prescriptions
                .CountAsync(p =>
                    p.VisitDate >= today &&
                    p.VisitDate < tomorrow);


        var recentPrescriptions =
            await prescriptions

                .Include(p => p.Patient)

                .OrderByDescending(
                    p => p.VisitDate)

                .ThenByDescending(
                    p => p.PrescriptionId)

                .Take(5)

                .ToListAsync();


        var model =
            new DoctorDashboardViewModel
            {
                DoctorName =
                    doctor.FullName,

                DoctorImage =
                    doctor.ImagePath,

                Speciality =
                    doctor.Speciality.ToString(),

                TotalPatients =
                    totalPatients,

                TotalPrescriptions =
                    totalPrescriptions,

                TodayPrescriptions =
                    todayPrescriptions,

                RecentPrescriptions =
                    recentPrescriptions
            };


        return View(model);
    }







    public async Task<IActionResult> Index(
    string? search,
    int page = 1,
    int pageSize = 6)
    {
        if (page < 1)
            page = 1;

        if (pageSize < 1)
            pageSize = 6;


        IQueryable<Doctor> query =
            _context.Doctors
                .Where(d => !d.IsDeleted);


        if (User.IsInRole("Doctor"))
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Forbid();

            query = query.Where(d => d.UserId == user.Id);
        }


        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(d =>
                d.FirstName.Contains(search) ||
                d.LastName.Contains(search) ||
                d.Phone.Contains(search) ||
                (d.Email != null &&
                 d.Email.Contains(search)));
        }


        var totalDoctors =
            await query.CountAsync();


        var doctors =
            await query
                .OrderBy(d => d.DoctorId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


        ViewBag.Search = search;

        ViewBag.CurrentPage = page;

        ViewBag.PageSize = pageSize;

        ViewBag.TotalPages =
            (int)Math.Ceiling(
                (double)totalDoctors /
                pageSize);


        return View(doctors);
    }




    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();


        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d =>
                d.DoctorId == id &&
                !d.IsDeleted);


        if (doctor == null)
            return NotFound();



        if (User.IsInRole("Doctor"))
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null ||
                doctor.UserId != user.Id)
            {
                return Forbid();
            }
        }


        return View(doctor);
    }




    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View(new CreateDoctorViewModel());
    }




    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(
        CreateDoctorViewModel model,
        [FromServices] IWebHostEnvironment env)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }


        var temporaryPassword =
            GenerateTemporaryPassword();


        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true,
            MustChangePassword = true,
            PhoneNumber = model.Phone,
            IsActive = true
        };


        var userResult = await _userManager.CreateAsync(
            user,
            temporaryPassword);


        if (!userResult.Succeeded)
        {
            foreach (var error in userResult.Errors)
            {
                ModelState.AddModelError(
                    "",
                    error.Description);
            }

            return View(model);
        }


        var roleResult = await _userManager.AddToRoleAsync(
            user,
            "Doctor");


        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            foreach (var error in roleResult.Errors)
            {
                ModelState.AddModelError(
                    "",
                    error.Description);
            }

            return View(model);
        }


        var doctor = new Doctor
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Speciality = model.Speciality,
            Age = model.Age,
            Phone = model.Phone,
            Email = model.Email,

            UserId = user.Id,

            IsDeleted = false,
            DeletedAt = null
        };


        if (model.Upload is not null)
        {
            var fileName =
                $"/Images/DoctorsImage_{Guid.NewGuid()}_{model.Upload.FileName}";


            using Stream stream =
                System.IO.File.Create(
                    env.WebRootPath + fileName);


            await model.Upload.CopyToAsync(stream);


            doctor.ImagePath = fileName;
        }


        _context.Doctors.Add(doctor);

        await _context.SaveChangesAsync();


        TempData["DoctorEmail"] = model.Email;
        TempData["TemporaryPassword"] =
            temporaryPassword;


        return RedirectToAction(nameof(Index));
    }




    private string GenerateTemporaryPassword()
    {
        return $"Doc@{Guid.NewGuid().ToString("N")[..8]}9!";
    }




    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();


        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d =>
                d.DoctorId == id &&
                !d.IsDeleted);


        if (doctor == null)
            return NotFound();


        if (User.IsInRole("Doctor"))
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null ||
                doctor.UserId != user.Id)
            {
                return Forbid();
            }
        }


        return View(doctor);
    }





    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> Edit(
        int? doctorid,
        Doctor doctor,
        [FromServices] IWebHostEnvironment env)
    {
        if (doctorid != doctor.DoctorId)
            return NotFound();


        var existingDoctor = await _context.Doctors
            .FirstOrDefaultAsync(d =>
                d.DoctorId == doctor.DoctorId &&
                !d.IsDeleted);


        if (existingDoctor == null)
            return NotFound();


        if (User.IsInRole("Doctor"))
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null ||
                existingDoctor.UserId != user.Id)
            {
                return Forbid();
            }
        }


        if (!ModelState.IsValid)
            return View(doctor);


        try
        {
            existingDoctor.FirstName =
                doctor.FirstName;

            existingDoctor.LastName =
                doctor.LastName;

            existingDoctor.Speciality =
                doctor.Speciality;

            existingDoctor.Age =
                doctor.Age;

            existingDoctor.Phone =
                doctor.Phone;


            if (doctor.Upload != null)
            {
                existingDoctor.Upload =
                    doctor.Upload;

                existingDoctor.SaveDoctorImage(env);
            }


            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!DoctorExists(doctor.DoctorId))
                return NotFound();

            throw;
        }


        return RedirectToAction(nameof(Index));
    }




    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();


        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d =>
                d.DoctorId == id &&
                !d.IsDeleted);


        if (doctor == null)
            return NotFound();


        return View(doctor);
    }





    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        if (id == null)
            return NotFound();


        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d =>
                d.DoctorId == id &&
                !d.IsDeleted);


        if (doctor == null)
            return NotFound();


        var hasPrescription =
            await _context.Prescriptions
                .AnyAsync(p =>
                    p.DoctorId == doctor.DoctorId);


        if (hasPrescription)
        {
            doctor.IsDeleted = true;
            doctor.DeletedAt = DateTime.Now;


            if (!string.IsNullOrEmpty(doctor.UserId))
            {
                var user =
                    await _userManager.FindByIdAsync(
                        doctor.UserId);

                if (user != null)
                {
                    user.IsActive = false;
                }
            }
        }
        else
        {

            _context.Doctors.Remove(doctor);


            if (!string.IsNullOrEmpty(doctor.UserId))
            {
                var user =
                    await _userManager.FindByIdAsync(
                        doctor.UserId);

                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }
            }
        }


        await _context.SaveChangesAsync();


        return RedirectToAction(nameof(Index));
    }


    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deleted()
    {
        var deletedDoctors = await _context.Doctors
            .Where(d => d.IsDeleted)
            .OrderByDescending(d => d.DeletedAt)
            .ToListAsync();


        return View(deletedDoctors);
    }




    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Restore(int? id)
    {
        if (id == null)
            return NotFound();

        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d =>
                d.DoctorId == id &&
                d.IsDeleted);

        if (doctor == null)
            return NotFound();

        return View(doctor);
    }




    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RestoreConfirmed(int? id)
    {
        if (id == null)
            return NotFound();

        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d =>
                d.DoctorId == id &&
                d.IsDeleted);

        if (doctor == null)
            return NotFound();


        doctor.IsDeleted = false;
        doctor.DeletedAt = null;

        if (!string.IsNullOrEmpty(doctor.UserId))
        {
            var user =
                await _userManager.FindByIdAsync(
                    doctor.UserId);

            if (user != null)
            {
                user.IsActive = true;
            }
        }


        await _context.SaveChangesAsync();


        return RedirectToAction(nameof(Deleted));
    }



    private bool DoctorExists(int? id)
    {
        return _context.Doctors
            .Any(d => d.DoctorId == id);
    }
}