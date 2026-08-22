using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Controllers
{
    [Authorize(Roles = "Patient")]
    public class PatientPortalController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly EmailService _emailService;

        public PatientPortalController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            EmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        // Get current patient record
        private async Task<Patient?> GetCurrentPatient()
        {
            var userId = _userManager.GetUserId(User);

            var account = await _context.PatientAccounts
                .Include(pa => pa.Patient)
                .FirstOrDefaultAsync(pa => pa.UserId == userId);

            return account?.Patient;
        }

        // Dashboard
        public async Task<IActionResult> Index()
        {
            var patient = await GetCurrentPatient();

            if (patient == null)
                return RedirectToAction("Login", "Account");

            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                .ThenInclude(d => d!.Department)
                .Where(a => a.PatientId == patient.Id)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            ViewBag.Patient = patient;

            ViewBag.TotalAppointments = appointments.Count;

            ViewBag.UpcomingAppointments = appointments
                .Count(a => a.AppointmentDate >= DateTime.Now);

            ViewBag.CompletedAppointments = appointments
                .Count(a => a.AppointmentDate < DateTime.Now);

            ViewBag.RecentAppointments = appointments
                .Take(5)
                .ToList();

            return View();
        }

        // My Appointments
        public async Task<IActionResult> MyAppointments()
        {
            var patient = await GetCurrentPatient();

            if (patient == null)
                return RedirectToAction("Login", "Account");

            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                .ThenInclude(d => d!.Department)
                .Where(a => a.PatientId == patient.Id)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            ViewBag.Patient = patient;

            return View(appointments);
        }

        // Book Appointment - GET
        public async Task<IActionResult> BookAppointment()
        {
            var doctors = await _context.Doctors
                .Include(d => d.Department)
                .ToListAsync();

            ViewBag.Doctors = new SelectList(
                doctors.Select(d => new
                {
                    Id = d.Id,
                    FullName = $"Dr. {d.FirstName} {d.LastName} — {d.Specialization}"
                }),
                "Id",
                "FullName");

            return View();
        }

        // Book Appointment - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookAppointment(
            int doctorId,
            DateTime appointmentDate,
            string notes)
        {
            var patient = await GetCurrentPatient();

            if (patient == null)
                return RedirectToAction("Login", "Account");

            try
            {
                // Insert appointment
                await _context.Database.ExecuteSqlRawAsync(
                    "INSERT INTO Appointments " +
                    "(AppointmentDate, Status, Notes, PatientId, DoctorId) " +
                    "VALUES ({0}, {1}, {2}, {3}, {4})",
                    appointmentDate,
                    "Pending",
                    notes ?? string.Empty,
                    patient.Id,
                    doctorId);

                // Get doctor details for email
                var doctor = await _context.Doctors
                    .Include(d => d.Department)
                    .FirstOrDefaultAsync(d => d.Id == doctorId);

                // Send confirmation email
                if (doctor != null)
                {
                    await _emailService.SendAppointmentConfirmationEmail(
                        patient.Email,
                        patient.FirstName + " " + patient.LastName,
                        doctor.FirstName + " " + doctor.LastName,
                        doctor.Specialization,
                        appointmentDate,
                        "Pending"
                    );
                }

                TempData["Success"] =
                    "Appointment booked! Confirmation email sent! ✅";

                return RedirectToAction(nameof(MyAppointments));
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }

            // Reload doctors if there is an error
            var doctors = await _context.Doctors
                .Include(d => d.Department)
                .ToListAsync();

            ViewBag.Doctors = new SelectList(
                doctors.Select(d => new
                {
                    Id = d.Id,
                    FullName = $"Dr. {d.FirstName} {d.LastName} — {d.Specialization}"
                }),
                "Id",
                "FullName");

            return View();
        }

        // My Profile
        public async Task<IActionResult> MyProfile()
        {
            var patient = await GetCurrentPatient();

            if (patient == null)
                return RedirectToAction("Login", "Account");

            return View(patient);
        }

        // Update Profile
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(Patient model)
        {
            try
            {
                var patient = await GetCurrentPatient();

                if (patient == null)
                    return RedirectToAction("Login", "Account");

                // Use raw SQL to avoid EF tracking issues
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE Patients SET PhoneNumber={0}, Address={1}, BloodType={2} WHERE Id={3}",
                    model.PhoneNumber ?? string.Empty,
                    model.Address ?? string.Empty,
                    model.BloodType ?? string.Empty,
                    patient.Id);

                TempData["Success"] =
                    "Profile updated successfully! ✅";
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "Error updating profile: " + ex.Message;
            }

            return RedirectToAction(nameof(MyProfile));
        }

        // Cancel Appointment
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var patient = await GetCurrentPatient();

            if (patient == null)
                return RedirectToAction("Login", "Account");

            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE Appointments SET Status = {0} " +
                "WHERE Id = {1} AND PatientId = {2}",
                "Cancelled",
                id,
                patient.Id);

            TempData["Success"] =
                "Appointment cancelled successfully!";

            return RedirectToAction(nameof(MyAppointments));
        }
    }
}