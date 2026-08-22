using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public SettingsController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.Role = roles.FirstOrDefault() ?? "No Role";
            return View(user);
        }

        // Change Password
        [HttpPost]
        public async Task<IActionResult> ChangePassword(
            string currentPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "New passwords do not match!";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var result = await _userManager.ChangePasswordAsync(
                user, currentPassword, newPassword);

            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["Success"] = "Password changed successfully!";
            }
            else
            {
                TempData["Error"] = string.Join(", ",
                    result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(Index));
        }

        // Delete Account
        [HttpPost]
        public async Task<IActionResult> DeleteAccount(string password)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var checkPassword = await _userManager
                .CheckPasswordAsync(user, password);

            if (!checkPassword)
            {
                TempData["Error"] = "Incorrect password!";
                return RedirectToAction(nameof(Index));
            }

            await _signInManager.SignOutAsync();
            await _userManager.DeleteAsync(user);
            TempData["Success"] = "Account deleted successfully!";
            return RedirectToAction("Login", "Account");
        }
    }
}