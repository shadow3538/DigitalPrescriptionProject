using DigitalPrescriptionProject.Data;
using DigitalPrescriptionProject.Models;
using DigitalPrescriptionProject.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DigitalPrescriptionProject.Controllers;

[Authorize(Roles = "Admin")]
public class UserManagementController : Controller
{

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public UserManagementController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        IWebHostEnvironment env)
    {
        _userManager = userManager;
        _context = context;
        _env = env;
    }

    // GET: UserManagement/Create
    public IActionResult Create()
    {
        return View(new CreateUserViewModel());
    }

    // POST: UserManagement/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string[] allowedRoles =
{
    "Doctor",
    "Patient",
    "Receptionist"
};

        if (!allowedRoles.Contains(model.Role))
        {
            ModelState.AddModelError(
                "Role",
                "Invalid role selected.");

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
            model.Password);

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
            model.Role);

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


        if (model.Role == "Doctor")
        {
            if (model.Speciality == null ||
                model.Age == null)
            {
                ModelState.AddModelError(
                    "",
                    "Doctor speciality and age are required.");

                await _userManager.RemoveFromRoleAsync(
                    user,
                    "Doctor");

                await _userManager.DeleteAsync(user);

                return View(model);
            }

            var doctor = new Doctor
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Speciality = model.Speciality.Value,
                Age = model.Age.Value,
                Phone = model.Phone,
                Email = model.Email,
                UserId = user.Id
            };

            doctor.Upload = model.Upload;
            doctor.SaveDoctorImage(_env);

            _context.Doctors.Add(doctor);
        }

        if (model.Role == "Patient")
        {
            if (model.Gender == null ||
                model.Age == null)
            {
                ModelState.AddModelError(
                    "",
                    "Patient gender and age are required.");

                await _userManager.RemoveFromRoleAsync(
                    user,
                    "Patient");

                await _userManager.DeleteAsync(user);

                return View(model);
            }

            var patient = new Patient
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Age = model.Age.Value,
                Phone = model.Phone,
                Email = model.Email,
                Gender = model.Gender.Value,
                UserId = user.Id,
                CreatedAt = DateTime.Now
            };

            patient.Upload = model.Upload;
            patient.SavePatientImage(_env);

            _context.Patients.Add(patient);
        }


        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Create));
    }
}