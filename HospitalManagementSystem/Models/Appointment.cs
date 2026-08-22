using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Appointment Date")]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;

        public int PatientId { get; set; }
        public Patient Patient { get; set; } = new Patient();

        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = new Doctor();
    }
}