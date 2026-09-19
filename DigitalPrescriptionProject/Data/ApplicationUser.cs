using Microsoft.AspNetCore.Identity;

namespace DigitalPrescriptionProject.Data;

public class ApplicationUser : IdentityUser
{
    public bool MustChangePassword { get; set; } = true;

    public bool IsActive { get; set; } = true;
}