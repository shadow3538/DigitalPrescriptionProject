
using DigitalPrescriptionProject.Data;
using DigitalPrescriptionProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

[Authorize (Roles = "Admin, Doctor, Patient")]
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

    // GET: PRESCRIPTIONS
    public async Task<IActionResult> Index()
    {
        IQueryable<Prescription> query = _context.Prescriptions
            .Include(p => p.Doctor)
            .Include(p => p.Patient)
            .Include(p => p.PrescriptionItems)
            .Include(p => p.PrescribedTests);

        if (User.IsInRole("Doctor"))
        {
            var doctor = await GetCurrentDoctorAsync();

            if (doctor == null)
                return Forbid();

            query = query.Where(p => p.DoctorId == doctor.DoctorId);
        }
        else if (User.IsInRole("Patient"))
        {
            var patient = await GetCurrentPatientAsync();

            if (patient == null)
                return Forbid();

            query = query.Where(p => p.PatientId == patient.PatientId);
        }

        return View(await query.ToListAsync());
    }

    // GET: PRESCRIPTIONS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var prescription = await _context.Prescriptions.Include(a=>a.Patient).Include(e=>e.Doctor).Include(t=>t.PrescribedTests).Include(i=>i.PrescriptionItems)
            .FirstOrDefaultAsync(m => m.PrescriptionId == id);
        if (prescription == null)
            return NotFound();

        if (User.IsInRole("Doctor"))
        {
            var doctor = await GetCurrentDoctorAsync();

            if (doctor == null || prescription.DoctorId != doctor.DoctorId)
                return Forbid();
        }

        if (User.IsInRole("Patient"))
        {
            var patient = await GetCurrentPatientAsync();

            if (patient == null || prescription.PatientId != patient.PatientId)
                return Forbid();
        }
       

        return View(prescription);
    }

    // GET: PRESCRIPTIONS/Create
    [Authorize (Roles = "Doctor, Admin")]
    public IActionResult Create()
    {
        patientDropDown();
        DoctorDropDown();
        return View(new Prescription());
    }

    private void patientDropDown(object select = null)
    {
        ViewBag.PatientId= new SelectList(_context.Patients.ToList(), "PatientId", "FullName", select);
    }

    private void DoctorDropDown(object select = null)
    {
        ViewBag.DoctorId = new SelectList(_context.Doctors.ToList(), "DoctorId", "FullName", select);
    }


    // POST: PRESCRIPTIONS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
            var doctor = await GetCurrentDoctorAsync();

            if (doctor == null)
                return Forbid();

            prescription.DoctorId = doctor.DoctorId;
        }

        patientDropDown(prescription.PatientId);
        DoctorDropDown(prescription.DoctorId);

 
        if (treatmentOperation.ToLower() == "add")
        {
            prescription.PrescriptionItems.Add(new());
            return View(prescription);
        }

       
        if (treatmentOperation.StartsWith("delete-"))
        {
            int.TryParse(
                treatmentOperation.Replace("delete-", ""),
                out int index);

            if (index >= 0 &&
                index < prescription.PrescriptionItems.Count)
            {
                prescription.PrescriptionItems.RemoveAt(index);
            }

            ModelState.Clear();
            return View(prescription);
        }


        if (testOperation.ToLower() == "add")
        {
            prescription.PrescribedTests.Add(new());
            return View(prescription);
        }

       
        if (testOperation.StartsWith("delete-"))
        {
            int.TryParse(
                testOperation.Replace("delete-", ""),
                out int index);

            if (index >= 0 &&
                index < prescription.PrescribedTests.Count)
            {
                prescription.PrescribedTests.RemoveAt(index);
            }

            ModelState.Clear();
            return View(prescription);
        }

        if (ModelState.IsValid)
        {
            _context.Add(prescription);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        return View(prescription);
    }

    // GET: PRESCRIPTIONS/Edit/5
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var prescription = await _context.Prescriptions
            .Include(p => p.PrescribedTests)
            .Include(p => p.PrescriptionItems)
            .Include(p => p.Doctor)
            .Include(p => p.Patient)
            .FirstOrDefaultAsync(p => p.PrescriptionId == id);

        if (prescription == null)
        {
            return NotFound();
        }

        
        if (User.IsInRole("Doctor"))
        {
            var doctor = await GetCurrentDoctorAsync();

            if (doctor == null || prescription.DoctorId != doctor.DoctorId)
            {
                return Forbid();
            }
        }

        patientDropDown(prescription.PatientId);
        DoctorDropDown(prescription.DoctorId);

        return View(prescription);
    }

    // POST: PRESCRIPTIONS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles ="Doctor, Admin")]
    public async Task<IActionResult> Edit(
    Prescription prescription,
    [FromServices] IWebHostEnvironment env,
    string treatmentOperation = "save",
    string testOperation = "save")
    {
        // Doctor শুধু নিজের prescription edit করতে পারবে
        if (User.IsInRole("Doctor"))
        {
            var doctor = await GetCurrentDoctorAsync();

            if (doctor == null)
                return Forbid();

            var existingPrescription = await _context.Prescriptions
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    p => p.PrescriptionId == prescription.PrescriptionId);

            if (existingPrescription == null)
                return NotFound();

            if (existingPrescription.DoctorId != doctor.DoctorId)
                return Forbid();

            // Posted DoctorId trust করা যাবে না
            prescription.DoctorId = doctor.DoctorId;
        }
        patientDropDown(prescription.PatientId);
        DoctorDropDown(prescription.DoctorId);

        if (treatmentOperation.ToLower() == "add")
        {
            prescription.PrescriptionItems.Add(new());
            return View(prescription);
        }

        if (treatmentOperation.StartsWith("delete-"))
        {
            int.TryParse(
                treatmentOperation.Replace("delete-", ""),
                out int index);

            if (index >= 0 &&
                index < prescription.PrescriptionItems.Count)
            {
                prescription.PrescriptionItems.RemoveAt(index);
            }

            ModelState.Clear();
            return View(prescription);
        }

        if (testOperation.ToLower() == "add")
        {
            prescription.PrescribedTests.Add(new());
            return View(prescription);
        }

        if (testOperation.StartsWith("delete-"))
        {
            int.TryParse(
                testOperation.Replace("delete-", ""),
                out int index);

            if (index >= 0 &&
                index < prescription.PrescribedTests.Count)
            {
                prescription.PrescribedTests.RemoveAt(index);
            }

            ModelState.Clear();
            return View(prescription);
        }

        if (ModelState.IsValid)
        {
            try
            {
               
                var testIds = prescription.PrescribedTests
                    .Select(x => x.Id)
                    .ToList();

            
                var itemIds = prescription.PrescriptionItems
                    .Select(x => x.PrescriptionItemId)
                    .ToList();


                var oldTests = _context.PrescribedTests
                    .Where(x =>
                        x.PrescriptionId == prescription.PrescriptionId &&
                        !testIds.Contains(x.Id));

                _context.PrescribedTests.RemoveRange(oldTests);


                var oldItems = _context.PrescriptionItems
                    .Where(x =>
                        x.PrescriptionId == prescription.PrescriptionId &&
                        !itemIds.Contains(x.PrescriptionItemId));

                _context.PrescriptionItems.RemoveRange(oldItems);

                
                foreach (var item in prescription.PrescribedTests)
                {
                    item.PrescriptionId = prescription.PrescriptionId;

                    _context.Entry(item).State =
                        item.Id == 0
                            ? EntityState.Added
                            : EntityState.Modified;
                }


                foreach (var item in prescription.PrescriptionItems)
                {
                    item.PrescriptionId = prescription.PrescriptionId;

                    _context.Entry(item).State =
                        item.PrescriptionItemId == 0
                            ? EntityState.Added
                            : EntityState.Modified;
                }


                _context.Entry(prescription).State =
                    EntityState.Modified;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PrescriptionExists(prescription.PrescriptionId))
                {
                    return NotFound();
                }

                throw;
            }
        }

        return View(prescription);
    }

    // GET: PRESCRIPTIONS/Delete/5
    [Authorize("Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var prescription = await _context.Prescriptions
            .FirstOrDefaultAsync(m => m.PrescriptionId == id);
        if (prescription == null)
        {
            return NotFound();
        }
        _context.Entry(prescription).Reference(p => p.Doctor).Load();
        _context.Entry(prescription).Reference(p => p.Patient).Load();
        return View(prescription);
    }

    // POST: PRESCRIPTIONS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize("Admin")]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var prescription = await _context.Prescriptions.FindAsync(id);
        if (prescription != null)
        {
            _context.Prescriptions.Remove(prescription);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }


    private async Task<Doctor?> GetCurrentDoctorAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return null;

        return await _context.Doctors
            .FirstOrDefaultAsync(d => d.UserId == user.Id);
    }

    private async Task<Patient?> GetCurrentPatientAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return null;

        return await _context.Patients
            .FirstOrDefaultAsync(p => p.UserId == user.Id);
    }




    private bool PrescriptionExists(int? id)
    {
        return _context.Prescriptions.Any(e => e.PrescriptionId == id);
    }
}
