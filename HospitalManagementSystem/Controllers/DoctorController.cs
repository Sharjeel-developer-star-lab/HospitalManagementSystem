using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Staff,Viewer")]
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public DoctorController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index()
        {
            var doctors = await _context.Doctors
                .Include(d => d.Department)
                .ToListAsync();
            return View(doctors);
        }

        public async Task<IActionResult> Create()
        {
            var departments = await _context.Departments.ToListAsync();
            ViewBag.Departments = new SelectList(departments, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Doctor doctor)
        {
            ModelState.Remove("Department");
            ModelState.Remove("Appointments");

            if (ModelState.IsValid)
            {
                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();

                await _emailService.SendDoctorWelcomeEmail(
                    doctor.Email,
                    doctor.FirstName + " " + doctor.LastName,
                    doctor.Specialization
                );

                TempData["Success"] = "Doctor added successfully!";
                return RedirectToAction(nameof(Index));
            }

            var departments = await _context.Departments.ToListAsync();
            ViewBag.Departments = new SelectList(departments, "Id", "Name");
            return View(doctor);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return NotFound();
            ViewBag.Departments = new SelectList(
                await _context.Departments.ToListAsync(),
                "Id", "Name", doctor.DepartmentId);
            return View(doctor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Doctor doctor)
        {
            ModelState.Remove("Department");
            ModelState.Remove("Appointments");

            if (ModelState.IsValid)
            {
                _context.Doctors.Update(doctor);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Doctor updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Departments = new SelectList(
                await _context.Departments.ToListAsync(),
                "Id", "Name", doctor.DepartmentId);
            return View(doctor);
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // First delete related appointments
                var appointments = _context.Appointments
                    .Where(a => a.DoctorId == id);
                _context.Appointments.RemoveRange(appointments);
                await _context.SaveChangesAsync();

                // Then delete doctor
                var doctor = await _context.Doctors.FindAsync(id);
                if (doctor == null) return NotFound();
                _context.Doctors.Remove(doctor);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Doctor deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Cannot delete: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.Department)
                .Include(d => d.Appointments)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (doctor == null) return NotFound();
            return View(doctor);
        }
    }
}