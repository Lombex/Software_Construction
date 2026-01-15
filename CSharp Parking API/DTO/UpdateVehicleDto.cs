using System.ComponentModel.DataAnnotations;

namespace CSharpAPI.Models
{
    public class UpdateVehicleDto
    {
        [MaxLength(10)]
        public string? license_plate { get; set; }

        public string? make { get; set; }

        public string? model { get; set; }

        public string? color { get; set; }

        public DateTime? year { get; set; }
    }
}
