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
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public AppointmentController(
            ApplicationDbContext context,
            EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // =========================
        // APPOINTMENT LIST
        // =========================
        public async Task<IActionResult> Index()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .ToListAsync();

            return View(appointments);
        }

        // =========================
        // CREATE - GET
        // =========================
        public async Task<IActionResult> Create()
        {
            var patients = await _context.Patients.ToListAsync();
            var doctors = await _context.Doctors.ToListAsync();

            ViewBag.Patients = new SelectList(
                patients.Select(p => new
                {
                    Id = p.Id,
                    FullName = p.FirstName + " " + p.LastName
                }),
                "Id",
                "FullName");

            ViewBag.Doctors = new SelectList(
                doctors.Select(d => new
                {
                    Id = d.Id,
                    FullName = "Dr. " + d.FirstName + " " + d.LastName
                }),
                "Id",
                "FullName");

            return View();
        }

        // =========================
        // CREATE - POST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Appointment appointment)
        {
            try
            {
                string notes = appointment.Notes ?? string.Empty;

                await _context.Database.ExecuteSqlRawAsync(
                    "INSERT INTO Appointments " +
                    "(AppointmentDate, Status, Notes, PatientId, DoctorId) " +
                    "VALUES ({0}, {1}, {2}, {3}, {4})",
                    appointment.AppointmentDate,
                    appointment.Status,
                    notes,
                    appointment.PatientId,
                    appointment.DoctorId
                );

                // Get patient
                var patient = await _context.Patients
                    .FindAsync(appointment.PatientId);

                // Get doctor
                var doctor = await _context.Doctors
                    .FindAsync(appointment.DoctorId);

                // Send email
                if (patient != null && doctor != null)
                {
                    await _emailService.SendAppointmentConfirmationEmail(
                        patient.Email,
                        patient.FirstName + " " + patient.LastName,
                        doctor.FirstName + " " + doctor.LastName,
                        doctor.Specialization,
                        appointment.AppointmentDate,
                        appointment.Status
                    );
                }

                TempData["Success"] =
                    "Appointment booked! Confirmation email sent! ✅";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message
                    + " | INNER: "
                    + ex.InnerException?.Message
                    + " | INNER2: "
                    + ex.InnerException?.InnerException?.Message;
            }

            var patients = await _context.Patients.ToListAsync();
            var doctors = await _context.Doctors.ToListAsync();

            ViewBag.Patients = new SelectList(
                patients.Select(p => new
                {
                    Id = p.Id,
                    FullName = p.FirstName + " " + p.LastName
                }),
                "Id",
                "FullName");

            ViewBag.Doctors = new SelectList(
                doctors.Select(d => new
                {
                    Id = d.Id,
                    FullName = "Dr. " + d.FirstName + " " + d.LastName
                }),
                "Id",
                "FullName");

            return View(appointment);
        }

        // =========================
        // EDIT - GET
        // =========================
        public async Task<IActionResult> Edit(int id)
        {
            var appointment = await _context.Appointments
                .FindAsync(id);

            if (appointment == null)
                return NotFound();

            var patients = await _context.Patients.ToListAsync();
            var doctors = await _context.Doctors.ToListAsync();

            ViewBag.Patients = new SelectList(
                patients.Select(p => new
                {
                    Id = p.Id,
                    FullName = p.FirstName + " " + p.LastName
                }),
                "Id",
                "FullName",
                appointment.PatientId);

            ViewBag.Doctors = new SelectList(
                doctors.Select(d => new
                {
                    Id = d.Id,
                    FullName = "Dr. " + d.FirstName + " " + d.LastName
                }),
                "Id",
                "FullName",
                appointment.DoctorId);

            return View(appointment);
        }

        // =========================
        // EDIT - POST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Appointment appointment)
        {
            try
            {
                // Get the existing appointment BEFORE updating it
                var existingAppointment = await _context.Appointments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (existingAppointment == null)
                    return NotFound();

                string notes = appointment.Notes ?? string.Empty;

                // Update appointment
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE Appointments " +
                    "SET AppointmentDate={0}, " +
                    "Status={1}, " +
                    "Notes={2}, " +
                    "PatientId={3}, " +
                    "DoctorId={4} " +
                    "WHERE Id={5}",
                    appointment.AppointmentDate,
                    appointment.Status,
                    notes,
                    appointment.PatientId,
                    appointment.DoctorId,
                    id
                );

                // ==========================================
                // SEND EMAIL WHEN ADMIN CONFIRMS APPOINTMENT
                // ==========================================

                if (appointment.Status == "Confirmed" &&
                    existingAppointment.Status != "Confirmed")
                {
                    // Get patient details
                    var patient = await _context.Patients
                        .FindAsync(appointment.PatientId);

                    // Get doctor details
                    var doctor = await _context.Doctors
                        .FindAsync(appointment.DoctorId);

                    // Send confirmation email
                    if (patient != null && doctor != null)
                    {
                        await _emailService.SendAppointmentConfirmationEmail(
                            patient.Email,
                            patient.FirstName + " " + patient.LastName,
                            doctor.FirstName + " " + doctor.LastName,
                            doctor.Specialization,
                            appointment.AppointmentDate,
                            "Confirmed"
                        );
                    }
                }

                TempData["Success"] =
                    "Appointment updated successfully!";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message
                    + " | INNER: "
                    + ex.InnerException?.Message;
            }

            var patients = await _context.Patients.ToListAsync();
            var doctors = await _context.Doctors.ToListAsync();

            ViewBag.Patients = new SelectList(
                patients.Select(p => new
                {
                    Id = p.Id,
                    FullName = p.FirstName + " " + p.LastName
                }),
                "Id",
                "FullName",
                appointment.PatientId);

            ViewBag.Doctors = new SelectList(
                doctors.Select(d => new
                {
                    Id = d.Id,
                    FullName = "Dr. " + d.FirstName + " " + d.LastName
                }),
                "Id",
                "FullName",
                appointment.DoctorId);

            return View(appointment);
        }

        // =========================
        // DELETE
        // =========================
        public async Task<IActionResult> Delete(int id)
        {
            var appointment = await _context.Appointments
                .FindAsync(id);

            if (appointment == null)
                return NotFound();

            _context.Appointments.Remove(appointment);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Appointment deleted!";

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // DETAILS
        // =========================
        public async Task<IActionResult> Details(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
                return NotFound();

            return View(appointment);
        }
    }
}