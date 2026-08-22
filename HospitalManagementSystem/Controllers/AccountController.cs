using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly EmailService _emailService;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            EmailService emailService,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _context = context;
        }
        // =========================
        // Login
        // =========================

        // GET — Login
        public IActionResult Login()
        {
            if (User.Identity!.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        // POST — Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    var roles = await _userManager.GetRolesAsync(user!);

                    TempData["Success"] = "Welcome back! 👋";

                    if (roles.Contains("Patient"))
                        return RedirectToAction("Index", "PatientPortal");
                    else
                        return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Invalid email or password!");
            }

            return View(model);
        }

        // =========================
        // Register
        // =========================

        // GET — Register
        public IActionResult Register()
        {
            if (User.Identity!.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        // POST — Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    if (model.AccountType == "Patient")
                    {
                        // Give Patient role
                        await _userManager.AddToRoleAsync(user, "Patient");

                        // Create Patient record
                        var patient = new Patient
                        {
                            FirstName = model.FullName.Split(' ')[0],
                            LastName = model.FullName.Contains(' ')
                                ? model.FullName.Substring(model.FullName.IndexOf(' ') + 1)
                                : "",
                            Email = model.Email,
                            PhoneNumber = model.PhoneNumber ?? "",
                            Gender = model.Gender ?? "",
                            DateOfBirth = model.DateOfBirth != null
                                ? DateTime.Parse(model.DateOfBirth)
                                : DateTime.Now,
                            Address = "",
                            BloodType = "",
                            RegistrationDate = DateTime.Now
                        };

                        _context.Patients.Add(patient);
                        await _context.SaveChangesAsync();

                        // Link patient to user account
                        var patientAccount = new PatientAccount
                        {
                            UserId = user.Id,
                            PatientId = patient.Id,
                            FullName = model.FullName,
                            CreatedAt = DateTime.Now
                        };

                        _context.PatientAccounts.Add(patientAccount);
                        await _context.SaveChangesAsync();

                        // Send welcome email
                        await _emailService.SendPatientWelcomeEmail(
                            model.Email, model.FullName);

                        await _signInManager.SignInAsync(user, isPersistent: false);
                        TempData["Success"] = "Welcome to HospitalMS! 🎉";
                        return RedirectToAction("Index", "PatientPortal");
                    }
                    else
                    {
                        // Staff account
                        await _userManager.AddToRoleAsync(user, "Viewer");

                        await _emailService.SendAccountWelcomeEmail(
                            model.Email, model.FullName);

                        await _signInManager.SignInAsync(user, isPersistent: false);
                        TempData["Success"] = "Account created! Welcome! 🎉";
                        return RedirectToAction("Index", "Home");
                    }
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }

        // =========================
        // Logout
        // =========================

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            TempData["Success"] = "Logged out successfully!";
            return RedirectToAction("Login");
        }

        // =========================
        // Forgot Password
        // =========================

        // GET
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["Error"] = "Please enter your email address!";
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                // Do not reveal whether the email exists
                TempData["Success"] = "If this email exists, a reset link has been sent!";
                return View();
            }

            // Generate password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Create reset link
            var resetLink = Url.Action(
                "ResetPassword",
                "Account",
                new { token, email },
                Request.Scheme);

            // Send reset email
            await _emailService.SendPasswordResetEmail(email, resetLink!);

            TempData["Success"] = "Password reset link sent to your email! ✅";
            return View();
        }

        // =========================
        // Reset Password
        // =========================

        // GET
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Invalid password reset request.";
                return RedirectToAction("ForgotPassword");
            }

            ViewBag.Token = token;
            ViewBag.Email = email;

            return View();
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(
            string token,
            string email,
            string newPassword,
            string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                TempData["Error"] = "Please fill in all fields.";

                ViewBag.Token = token;
                ViewBag.Email = email;

                return View();
            }

            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "Passwords do not match!";

                ViewBag.Token = token;
                ViewBag.Email = email;

                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                TempData["Error"] = "Invalid password reset request.";
                return View();
            }

            var result = await _userManager.ResetPasswordAsync(
                user,
                token,
                newPassword);

            if (result.Succeeded)
            {
                TempData["Success"] = "Password reset successfully! Please login.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            ViewBag.Token = token;
            ViewBag.Email = email;

            return View();
        }
    }
}