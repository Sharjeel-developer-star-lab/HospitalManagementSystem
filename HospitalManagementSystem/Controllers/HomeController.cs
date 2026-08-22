using HospitalManagementSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Staff,Viewer")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get statistics
            ViewBag.TotalPatients = await _context.Patients.CountAsync();
            ViewBag.TotalDoctors = await _context.Doctors.CountAsync();
            ViewBag.TotalAppointments = await _context.Appointments.CountAsync();
            ViewBag.TotalDepartments = await _context.Departments.CountAsync();

            // Get pending appointments
            ViewBag.PendingAppointments = await _context.Appointments
                .Where(a => a.Status == "Pending")
                .CountAsync();

            // Get confirmed appointments
            ViewBag.ConfirmedAppointments = await _context.Appointments
                .Where(a => a.Status == "Confirmed")
                .CountAsync();

            // Get recent appointments
            var recentAppointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .OrderByDescending(a => a.AppointmentDate)
                .Take(5)
                .ToListAsync();

            ViewBag.RecentAppointments = recentAppointments;

            // Get recent patients
            var recentPatients = await _context.Patients
                .OrderByDescending(p => p.RegistrationDate)
                .Take(5)
                .ToListAsync();

            ViewBag.RecentPatients = recentPatients;

            return View();
        }
    }
}
