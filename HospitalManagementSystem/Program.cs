using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration
    .GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSingleton<HospitalManagementSystem.Services.EmailService>();

// ✅ Cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
});

var app = builder.Build();

// Auto seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();
    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider
        .GetRequiredService<UserManager<IdentityUser>>();

    context.Database.EnsureCreated();

    // Seed Departments
    if (!context.Departments.Any())
    {
        context.Departments.AddRange(
            new Department { Name = "Cardiology", Description = "Heart and cardiovascular diseases" },
            new Department { Name = "Neurology", Description = "Brain and nervous system" },
            new Department { Name = "Dermatology", Description = "Skin conditions" },
            new Department { Name = "Pediatrics", Description = "Children health" },
            new Department { Name = "Surgery", Description = "Surgical procedures" },
            new Department { Name = "Orthopedics", Description = "Bones and joints" },
            new Department { Name = "Psychiatry", Description = "Mental health" },
            new Department { Name = "Radiology", Description = "Medical imaging" },
            new Department { Name = "Oncology", Description = "Cancer treatment" },
            new Department { Name = "Gynecology", Description = "Women health" },
            new Department { Name = "General Practice", Description = "General medicine" }
        );
        context.SaveChanges();
    }

    var radiologyId = context.Departments
        .First(d => d.Name == "Radiology").Id;
    var psychiatryId = context.Departments
        .First(d => d.Name == "Psychiatry").Id;

    // Seed Doctors
    if (!context.Doctors.Any())
    {
        context.Doctors.AddRange(
            new Doctor
            {
                FirstName = "John",
                LastName = "Smith",
                Email = "john.smith@hospital.com",
                PhoneNumber = "+353 1234567",
                Specialization = "Radiologist",
                YearsOfExperience = 5,
                DepartmentId = radiologyId
            },
            new Doctor
            {
                FirstName = "Sharjeel",
                LastName = "Ali",
                Email = "sharjeelikhlaq9@gmail.com",
                PhoneNumber = "+353876480073",
                Specialization = "Psychiatrist",
                YearsOfExperience = 13,
                DepartmentId = psychiatryId
            }
        );
        context.SaveChanges();
    }

    // Seed Patients
    if (!context.Patients.Any())
    {
        context.Patients.AddRange(
            new Patient
            {
                FirstName = "Ali",
                LastName = "Khan",
                DateOfBirth = new DateTime(1998, 3, 1),
                Gender = "Male",
                PhoneNumber = "0831726603",
                Email = "ali123@gmail.com",
                Address = "Dublin",
                BloodType = "A+",
                RegistrationDate = DateTime.Now
            },
            new Patient
            {
                FirstName = "Hassan",
                LastName = "Ali",
                DateOfBirth = new DateTime(1995, 5, 15),
                Gender = "Male",
                PhoneNumber = "3224900149",
                Email = "hassanali@gmail.com",
                Address = "Dublin",
                BloodType = "O+",
                RegistrationDate = DateTime.Now
            },
            new Patient
            {
                FirstName = "Stephen",
                LastName = "Mac",
                DateOfBirth = new DateTime(1990, 2, 10),
                Gender = "Male",
                PhoneNumber = "098765432",
                Email = "stephenmac@gmail.com",
                Address = "Dublin",
                BloodType = "B+",
                RegistrationDate = DateTime.Now
            }
        );
        context.SaveChanges();
    }

    // ✅ Seed Roles
    string[] roles = { "Admin", "Staff", "Viewer", "Patient" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // ✅ Seed Admin User
    string adminEmail = "admin@hospitalms.com";
    string adminPassword = "Admin@1234";

    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail
        };
        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// ✅ Protect all pages
app.Use(async (context, next) =>
{
    if (!context.User.Identity!.IsAuthenticated &&
        !context.Request.Path.StartsWithSegments("/Account") &&
        !context.Request.Path.StartsWithSegments("/lib") &&
        !context.Request.Path.StartsWithSegments("/css") &&
        !context.Request.Path.StartsWithSegments("/js"))
    {
        context.Response.Redirect("/Account/Login");
        return;
    }

    // Redirect patients to patient portal
    if (context.User.Identity.IsAuthenticated &&
        context.User.IsInRole("Patient") &&
        !context.Request.Path.StartsWithSegments("/PatientPortal") &&
        !context.Request.Path.StartsWithSegments("/Account") &&
        !context.Request.Path.StartsWithSegments("/Settings"))
    {
        context.Response.Redirect("/PatientPortal");
        return;
    }

    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();