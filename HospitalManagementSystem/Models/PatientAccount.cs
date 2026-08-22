using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class PatientAccount
    {
        public int Id { get; set; }

        // Links to Identity User
        [Required]
        public string UserId { get; set; } = string.Empty;

        // Links to Patient record
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }

        // Extra profile info
        [Required]
        public string FullName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}