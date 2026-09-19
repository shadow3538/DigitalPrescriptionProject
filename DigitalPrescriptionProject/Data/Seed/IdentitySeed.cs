using Microsoft.AspNetCore.Identity;

namespace DigitalPrescriptionProject.Data.Seed
{
    public static class IdentitySeed
    {
        public static async Task SeedRolesAsync(
            RoleManager<ApplicationRole> roleManager)
        {
            string[] roles =
            {
                "Admin",
                "Doctor",
                "Patient"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(
                        new ApplicationRole
                        {
                            Name = role
                        });
                }
            }
        }


        public static async Task SeedAdminUserAsync(
            UserManager<ApplicationUser> userManager)
        {
            var adminEmail =
                "admin@digitalprascription.com";

            var existingUser =
                await userManager.FindByEmailAsync(
                    adminEmail);


            if (existingUser != null)
            {
                bool changed = false;

                if (!existingUser.IsActive)
                {
                    existingUser.IsActive = true;
                    changed = true;
                }


                if (existingUser.MustChangePassword)
                {
                    existingUser.MustChangePassword = false;
                    changed = true;
                }


                if (changed)
                {
                    await userManager.UpdateAsync(
                        existingUser);
                }


                if (!await userManager.IsInRoleAsync(
                    existingUser,
                    "Admin"))
                {
                    await userManager.AddToRoleAsync(
                        existingUser,
                        "Admin");
                }


                return;
            }



            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,

                EmailConfirmed = true,

                IsActive = true,

                MustChangePassword = false
            };


            var result =
                await userManager.CreateAsync(
                    adminUser,
                    "Admin@12345");


            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(
                    adminUser,
                    "Admin");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    Console.WriteLine(
                        $"Identity Error: {error.Code} - {error.Description}");
                }
            }
        }
    }
}