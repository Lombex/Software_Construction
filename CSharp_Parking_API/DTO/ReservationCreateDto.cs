using System.ComponentModel.DataAnnotations;

namespace CSharpAPI.Models
{
    public class ReservationCreateDto
    {
        [Required]
        public Guid user_id { get; set; }

        [Required]
        public Guid parking_lot_id { get; set; }

        [Required]
        public Guid vehicle_id { get; set; }

        [Required]
        public DateTime start_time { get; set; }

        [Required]
        public DateTime end_time { get; set; }
    }
}
