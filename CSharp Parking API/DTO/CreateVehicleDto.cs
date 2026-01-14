using System.ComponentModel.DataAnnotations;

namespace CSharpAPI.Models
{
    public class CreateVehicleDto
    {
        public Guid? user_id { get; set; } // Optional - will be set automatically for non-admin users

        [Required]
        [MaxLength(10)]
        public string license_plate { get; set; } = string.Empty;

        [Required]
        public string make { get; set; } = string.Empty;

        [Required]
        public string model { get; set; } = string.Empty;

        public string? color { get; set; }

        [Required]
        public DateTime year { get; set; }
    }
}
