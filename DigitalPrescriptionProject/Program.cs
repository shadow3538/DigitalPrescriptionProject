using DigitalPrescriptionProject.Data;
using DigitalPrescriptionProject.Data.Seed;
using DigitalPrescriptionProject.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddTransient<IEmailSender, DevelopmentEmailSender>();

builder.Services.AddControllersWithViews();



builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{

    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    options.Lockout.DefaultLockoutTimeSpan =
        TimeSpan.FromMinutes(15);

    options.Lockout.MaxFailedAccessAttempts = 5;

    options.Lockout.AllowedForNewUsers = true;

    
    options.SignIn.RequireConfirmedAccount = true;
})
.AddRoles<ApplicationRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();


builder.Services.ConfigureApplicationCookie(options =>
{
    
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddControllersWithViews();

builder.Services.AddSerilog(op =>
{
    op.WriteTo.MSSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), new MSSqlServerSinkOptions()
    {
        TableName = "PrescriptionLogs",
        AutoCreateSqlDatabase = true,
        AutoCreateSqlTable = true

    }, restrictedToMinimumLevel : LogEventLevel.Warning);
});

var app = builder.Build();




if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}


app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();


app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();




using (var scope = app.Services.CreateScope())
{
    var roleManager =
        scope.ServiceProvider
            .GetRequiredService<RoleManager<ApplicationRole>>();

    var userManager =
        scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

    await IdentitySeed.SeedRolesAsync(roleManager);

    await IdentitySeed.SeedAdminUserAsync(userManager);
}


app.Run();