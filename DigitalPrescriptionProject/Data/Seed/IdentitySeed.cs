using Microsoft.AspNetCore.Identity;

namespace DigitalPrescriptionProject.Data.Seed
{
    public static class IdentitySeed
    {
        public static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManger)
        {
            string[] roles =
            {
                "Admin", "Doctor","Patient", "Receptionist"
            };

            foreach (var role in roles)
            {
               if(!await roleManger.RoleExistsAsync(role))
                {
                    await roleManger.CreateAsync(new ApplicationRole
                    {
                        Name = role
                    });
                }
            }
        }
        public static async Task SeedAdminUserAsync(
    UserManager<ApplicationUser> userManager)
        {
            var adminEmail = "admin@digitalprascription.com";

            var existingUser =
                await userManager.FindByEmailAsync(adminEmail);

            if (existingUser == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result =
                    await userManager.CreateAsync(
                        adminUser,
                        "Admin@12345"
                    );

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(
                        adminUser,
                        "Admin"
                    );
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine(
                            $"Identity Error: {error.Code} - {error.Description}"
                        );
                    }
                }
            }
        }
    }
    
}
