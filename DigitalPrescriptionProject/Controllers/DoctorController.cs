
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigitalPrescriptionProject.Models;
using DigitalPrescriptionProject.Data;
using Microsoft.AspNetCore.Identity;
using DigitalPrescriptionProject.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

[Authorize(Roles = ("Admin, Doctor"))]
public class DoctorController : Controller
{
    private readonly ApplicationDbContext _context;

    private readonly UserManager<ApplicationUser> _userManager;

    public DoctorController(ApplicationDbContext context, UserManager<ApplicationUser> usermanager)
    {
        _context = context;
        _userManager = usermanager;
    }

    // GET: DOCTORS
    public async Task<IActionResult> Index()
    {
        if (User.IsInRole("Doctor"))
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Forbid();

            var doctors = await _context.Doctors
                .Where(d => d.UserId == user.Id)
                .ToListAsync();

            return View(doctors);
        }

        
        return View(await _context.Doctors.ToListAsync());
    }

    // GET: DOCTORS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(m => m.DoctorId == id);
        if (doctor == null)
        {
            return NotFound();
        }
        if (User.IsInRole("Doctor"))
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || doctor.UserId != user.Id)
                return Forbid();
        }

        return View(doctor);
    }

    // GET: DOCTORS/Create
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View(new CreateDoctorViewModel());
    }

    // POST: DOCTORS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
            "Doctor"
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

        var doctor = new Doctor
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Speciality = model.Speciality,
            Age = model.Age,
            Phone = model.Phone,
            Email = model.Email,
            UserId = user.Id
        };


        if (model.Upload is not null)
        {
            var fileName =
                $"/Images/DoctorsImage_{Guid.NewGuid()}_{model.Upload.FileName}";

            using Stream stream =
                System.IO.File.Create(
                    env.WebRootPath + fileName
                );

            await model.Upload.CopyToAsync(stream);

            doctor.ImagePath = fileName;
        }

        _context.Doctors.Add(doctor);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // GET: DOCTORS/Edit/5

    [Authorize(Roles = "Admin, Doctor")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var doctor = await _context.Doctors.FindAsync(id);
        if (doctor == null)
        {
            return NotFound();
        }
        if (User.IsInRole("Doctor"))
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || doctor.UserId != user.Id)
                return Forbid();
        }
        return View(doctor);
    }

    // POST: DOCTORS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles =("Admin, Doctor"))]
    public async Task<IActionResult> Edit(int? doctorid, Doctor doctor, [FromServices] IWebHostEnvironment env)
    {
        if (doctorid != doctor.DoctorId)
        {
            return NotFound();
        }

        if (User.IsInRole("Doctor"))
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Forbid();

            var existingDoctor = await _context.Doctors
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DoctorId == doctor.DoctorId);

            if (existingDoctor == null)
                return NotFound();

            if (existingDoctor.UserId != user.Id)
                return Forbid();

            
            doctor.UserId = existingDoctor.UserId;
        }

        if (ModelState.IsValid)
        {
            try
            {
                if (doctor.Upload is not null)
                {
                    var fileName = $"/Images/DoctorsImage_{Guid.NewGuid()}_{doctor.Upload.FileName}";
                    using Stream stream = System.IO.File.Create(env.WebRootPath + fileName);
                    await doctor.Upload.CopyToAsync(stream);
                    doctor.ImagePath = fileName;
                }
                _context.Update(doctor);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DoctorExists(doctor.DoctorId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(doctor);
    }

    // GET: DOCTORS/Delete/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(m => m.DoctorId == id);
        if (doctor == null)
        {
            return NotFound();
        }

        return View(doctor);
    }

    // POST: DOCTORS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var doctor = await _context.Doctors.FindAsync(id);
        if (doctor != null)
        {
            _context.Doctors.Remove(doctor);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool DoctorExists(int? id)
    {
        return _context.Doctors.Any(e => e.DoctorId == id);
    }
}
