using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using DigitalPrescriptionProject.Data;
using DigitalPrescriptionProject.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace DigitalPrescriptionProject.Areas.Identity.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserStore<ApplicationUser> _userStore;
    private readonly IUserEmailStore<ApplicationUser> _emailStore;
    private readonly ILogger<RegisterModel> _logger;
    private readonly IEmailSender _emailSender;
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public RegisterModel(
        UserManager<ApplicationUser> userManager,
        IUserStore<ApplicationUser> userStore,
        SignInManager<ApplicationUser> signInManager,
        ILogger<RegisterModel> logger,
        IEmailSender emailSender,
        ApplicationDbContext context,
        IWebHostEnvironment environment)
    {
        _userManager = userManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
        _signInManager = signInManager;
        _logger = logger;
        _emailSender = emailSender;
        _context = context;
        _environment = environment;
    }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public string? ReturnUrl { get; set; }

    public IList<AuthenticationScheme>? ExternalLogins { get; set; }

    public class InputModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = "";

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = "";

        [Required]
        [Range(0, 150)]
        public int Age { get; set; }

        [Required]
        public Gender Gender { get; set; }

        [Required]
        public string Phone { get; set; } = "";

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        [Required]
        [StringLength(
            100,
            ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
            MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = "";

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare(
            "Password",
            ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }

        [Display(Name = "Profile Image")]
        public IFormFile? Upload { get; set; }
    }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;

        ExternalLogins =
            (await _signInManager
                .GetExternalAuthenticationSchemesAsync())
            .ToList();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        ExternalLogins =
            (await _signInManager
                .GetExternalAuthenticationSchemesAsync())
            .ToList();

        if (!ModelState.IsValid)
        {
            return Page();
        }


        var user = CreateUser();
        user.MustChangePassword = false;
        user.PhoneNumber = Input.Phone;

        await _userStore.SetUserNameAsync(
            user,
            Input.Email,
            CancellationToken.None);

        await _emailStore.SetEmailAsync(
            user,
            Input.Email,
            CancellationToken.None);

        var userResult =
            await _userManager.CreateAsync(
                user,
                Input.Password);

        if (!userResult.Succeeded)
        {
            foreach (var error in userResult.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return Page();
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

            return Page();
        }


        var patient = new Patient
        {
            FirstName = Input.FirstName,
            LastName = Input.LastName,
            Age = Input.Age,
            Gender = Input.Gender,
            Phone = Input.Phone,
            Email = Input.Email,
            UserId = user.Id,
            CreatedAt = DateTime.Today
        };


        patient.Upload = Input.Upload;

        try
        {
            patient.SavePatientImage(_environment);
        }
        catch (Exception ex)
        {
            await _userManager.DeleteAsync(user);

            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            return Page();
        }


        _context.Patients.Add(patient);

        var patientSaveResult =
            await _context.SaveChangesAsync();

        if (patientSaveResult <= 0)
        {
            await _userManager.DeleteAsync(user);

            ModelState.AddModelError(
                string.Empty,
                "Patient account could not be created.");

            return Page();
        }

        _logger.LogInformation(
            "Patient created a new account with password.");

        var userId =
            await _userManager.GetUserIdAsync(user);

        var code =
            await _userManager
                .GenerateEmailConfirmationTokenAsync(user);

        code = WebEncoders.Base64UrlEncode(
            Encoding.UTF8.GetBytes(code));

        var callbackUrl = Url.Page(
            "/Account/ConfirmEmail",
            pageHandler: null,
            values: new
            {
                area = "Identity",
                userId = userId,
                code = code,
                returnUrl = returnUrl
            },
            protocol: Request.Scheme)!;

        await _emailSender.SendEmailAsync(
            Input.Email,
            "Confirm your email",
            $"Please confirm your account by " +
            $"<a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>" +
            $"clicking here</a>.");


        if (_userManager.Options.SignIn.RequireConfirmedAccount)
        {
            return RedirectToPage(
                "RegisterConfirmation",
                new
                {
                    email = Input.Email,
                    returnUrl = returnUrl
                });
        }

        await _signInManager.SignInAsync(
            user,
            isPersistent: false);

        return LocalRedirect(returnUrl);
    }

    private ApplicationUser CreateUser()
    {
        try
        {
            return Activator.CreateInstance<ApplicationUser>();
        }
        catch
        {
            throw new InvalidOperationException(
                $"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class " +
                $"and has a parameterless constructor.");
        }
    }

    private IUserEmailStore<ApplicationUser> GetEmailStore()
    {
        if (!_userManager.SupportsUserEmail)
        {
            throw new NotSupportedException(
                "The default UI requires a user store with email support.");
        }

        return (IUserEmailStore<ApplicationUser>)_userStore;
    }
}