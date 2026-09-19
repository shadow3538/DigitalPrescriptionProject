// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using DigitalPrescriptionProject.Data;

namespace DigitalPrescriptionProject.Areas.Identity.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<LoginModel> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public LoginModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        ILogger<LoginModel> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }


    [BindProperty]
    public InputModel Input { get; set; } = default!;


    public IList<AuthenticationScheme>? ExternalLogins { get; set; }


    public string? ReturnUrl { get; set; }


    [TempData]
    public string? ErrorMessage { get; set; }


    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;


        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = default!;


        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }




    public async Task OnGetAsync(string? returnUrl = null)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(
                string.Empty,
                ErrorMessage);
        }

        returnUrl ??= Url.Content("~/");


        await HttpContext.SignOutAsync(
            IdentityConstants.ExternalScheme);


        ExternalLogins =
            (await _signInManager
                .GetExternalAuthenticationSchemesAsync())
            .ToList();


        ReturnUrl = returnUrl;
    }




    public async Task<IActionResult> OnPostAsync(
        string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");


        ExternalLogins =
            (await _signInManager
                .GetExternalAuthenticationSchemesAsync())
            .ToList();


        if (ModelState.IsValid)
        {
            var result =
                await _signInManager.PasswordSignInAsync(
                    Input.Email,
                    Input.Password,
                    Input.RememberMe,
                    lockoutOnFailure: true);


            if (result.Succeeded)
            {
                _logger.LogInformation(
                    "User logged in.");


                var user =
                    await _userManager
                        .FindByEmailAsync(Input.Email);


                if (user == null)
                {
                    return RedirectToPage(
                        "/Account/Login",
                        new
                        {
                            area = "Identity"
                        });
                }

                if (!user.IsActive)
                {
                    await _signInManager.SignOutAsync();

                    ModelState.AddModelError(
                        string.Empty,
                        "Your account is inactive. Please contact the administrator.");

                    return Page();
                }



                if (user.MustChangePassword)
                {
                    return RedirectToPage(
                        "/Account/Manage/ChangePassword",
                        new
                        {
                            area = "Identity"
                        });
                }


                if (await _userManager.IsInRoleAsync(
                    user,
                    "Doctor"))
                {
                    var doctor =
                        await _context.Doctors
                            .FirstOrDefaultAsync(
                                d => d.UserId == user.Id);


                    if (doctor == null)
                    {
                        await _signInManager.SignOutAsync();

                        return NotFound(
                            "Doctor profile not found.");
                    }


                    if (doctor.IsDeleted)
                    {
                        await _signInManager.SignOutAsync();

                        ModelState.AddModelError(
                            string.Empty,
                            "This doctor account is no longer active.");

                        return Page();
                    }


                    return RedirectToAction(
                        "Details",
                        "Doctor",
                        new
                        {
                            id = doctor.DoctorId
                        });
                }


                if (await _userManager.IsInRoleAsync(
                    user,
                    "Patient"))
                {
                    var patient =
                        await _context.Patients
                            .FirstOrDefaultAsync(
                                p => p.UserId == user.Id);


                    if (patient == null)
                    {
                        await _signInManager.SignOutAsync();

                        return NotFound(
                            "Patient profile not found.");
                    }


                    if (patient.IsDeleted)
                    {
                        await _signInManager.SignOutAsync();

                        ModelState.AddModelError(
                            string.Empty,
                            "This patient account is no longer active.");

                        return Page();
                    }


                    return RedirectToAction(
                        "Details",
                        "Patient",
                        new
                        {
                            id = patient.PatientId
                        });
                }


                if (await _userManager.IsInRoleAsync(
                    user,
                    "Admin"))
                {
                    return LocalRedirect(returnUrl);
                }


                return LocalRedirect(returnUrl);
            }


            if (result.RequiresTwoFactor)
            {
                return RedirectToPage(
                    "./LoginWith2fa",
                    new
                    {
                        ReturnUrl = returnUrl,
                        RememberMe = Input.RememberMe
                    });
            }


            if (result.IsLockedOut)
            {
                _logger.LogWarning(
                    "User account locked out.");

                return RedirectToPage(
                    "./Lockout");
            }


            ModelState.AddModelError(
                string.Empty,
                "Invalid login attempt.");

            return Page();
        }


        return Page();
    }
}