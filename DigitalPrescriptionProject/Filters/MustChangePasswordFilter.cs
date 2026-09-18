using DigitalPrescriptionProject.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DigitalPrescriptionProject.Filters
{
    public class MustChangePasswordFilter : IAsyncActionFilter
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public MustChangePasswordFilter(
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            if (context.HttpContext.User.Identity?.IsAuthenticated != true)
            {
                await next();
                return;
            }

            var user = await _userManager.GetUserAsync(
                context.HttpContext.User);

            if (user == null)
            {
                await next();
                return;
            }

            var isDoctor = await _userManager.IsInRoleAsync(
                user,
                "Doctor");

            var isPatient = await _userManager.IsInRoleAsync(
                user,
                "Patient");

            if (!isDoctor && !isPatient)
            {
                await next();
                return;
            }

            if (!user.MustChangePassword)
            {
                await next();
                return;
            }

            var path = context.HttpContext.Request.Path;

            if (path.StartsWithSegments(
                "/Identity/Account/Manage/ChangePassword"))
            {
                await next();
                return;
            }

            context.Result = new RedirectToPageResult(
                "/Account/Manage/ChangePassword",
                new
                {
                    area = "Identity"
                });
        }
    }
}

