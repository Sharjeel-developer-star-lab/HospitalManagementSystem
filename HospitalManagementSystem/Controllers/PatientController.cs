using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Staff,Viewer")]
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        public PatientController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index()
        {
            var patients = await _context.Patients.ToListAsync();
            return View(patients);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Patient patient)
        {
            ModelState.Remove("Appointments");
            if (ModelState.IsValid)
            {
                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();

                await _emailService.SendPatientWelcomeEmail(
                    patient.Email,
                    patient.FirstName + " " + patient.LastName
                );

                TempData["Success"] = "Patient registered successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(patient);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return NotFound();
            return View(patient);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Patient patient)
        {
            ModelState.Remove("Appointments");
            if (ModelState.IsValid)
            {
                _context.Patients.Update(patient);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Patient updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(patient);
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // First delete related appointments
                var appointments = _context.Appointments
                    .Where(a => a.PatientId == id);
                _context.Appointments.RemoveRange(appointments);
                await _context.SaveChangesAsync();

                // Then delete patient
                var patient = await _context.Patients.FindAsync(id);
                if (patient == null) return NotFound();
                _context.Patients.Remove(patient);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Patient deleted successfully!";
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
            var patient = await _context.Patients
                .Include(p => p.Appointments)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (patient == null) return NotFound();
            return View(patient);
        }
    }
}