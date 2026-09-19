using DigitalPrescriptionProject.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace DigitalPrescriptionProject.Filters
{
    public class ActiveUserFilter : IAsyncActionFilter
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ActiveUserFilter(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var user = await _userManager.GetUserAsync(
                context.HttpContext.User);

            if (user == null)
            {
                await next();
                return;
            }



            if (await _userManager.IsInRoleAsync(user, "Doctor"))
            {
                var doctor = await _context.Doctors
                    .FirstOrDefaultAsync(
                        d => d.UserId == user.Id);

                if (doctor == null || doctor.IsDeleted)
                {
                    await context.HttpContext.SignOutAsync();

                    context.Result = new RedirectToPageResult(
                        "/Account/Login",
                        new
                        {
                            area = "Identity"
                        });

                    return;
                }
            }


            if (await _userManager.IsInRoleAsync(user, "Patient"))
            {
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(
                        p => p.UserId == user.Id);

                if (patient == null || patient.IsDeleted)
                {
                    await context.HttpContext.SignOutAsync();

                    context.Result = new RedirectToPageResult(
                        "/Account/Login",
                        new
                        {
                            area = "Identity"
                        });

                    return;
                }
            }

            await next();
        }
    }
}