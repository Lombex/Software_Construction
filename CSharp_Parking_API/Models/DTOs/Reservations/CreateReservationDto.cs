using static CSharpAPI.Models.M_Reservations;

namespace CSharpAPI.Models.DTOs.Reservations
{
    // Minimal payload for creating a reservation from the API
    public class CreateReservationDto
    {
        public Guid id { get; set; }
        public Guid user_id { get; set; }
        public Guid vehicle_id { get; set; }
        public Guid parking_lot_id { get; set; }
        public DateTime start_time { get; set; }
        public DateTime end_time { get; set; }
        public Status status { get; set; }
        public DateTime created_at { get; set; }
        public float cost { get; set; }
    }
}
