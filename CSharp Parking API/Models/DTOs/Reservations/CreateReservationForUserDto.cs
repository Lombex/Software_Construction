using static CSharpAPI.Models.M_Reservations;

namespace CSharpAPI.Models.DTOs.Reservations
{
    // DTO for admins to create reservations for other users
    public class CreateReservationForUserDto
    {
        public Guid id { get; set; }
        public Guid user_id { get; set; } // User ID for whom the reservation is created
        public Guid vehicle_id { get; set; }
        public Guid parking_lot_id { get; set; }
        public DateTime start_time { get; set; }
        public DateTime end_time { get; set; }
        public Status status { get; set; } = Status.Active;
        public DateTime created_at { get; set; }
        public float cost { get; set; }
    }
}

