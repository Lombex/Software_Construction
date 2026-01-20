using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CSharpAPI.Models
{
    public class M_Reservations
    {
        public enum Status
        {
            Active,
            Cancelled,
            Completed
        }
        [Key]
        public Guid id { get; set; }

        // FK properties (Guid)
        public Guid user_id { get; set; }
        public Guid parking_lot_id { get; set; }
        public Guid vehicle_id { get; set; }

        // navigatie-eigenschappen
        [ForeignKey(nameof(user_id))]
        public M_Users ?user { get; set; }

        [ForeignKey(nameof(parking_lot_id))]
        public M_Parkinglots ?parking_lot { get; set; }

        [ForeignKey(nameof(vehicle_id))]
        public M_Vehicles ?vehicle { get; set; }

        public DateTime start_time { get; set; }
        public DateTime end_time { get; set; }
        public Status status { get; set; }
        public DateTime created_at { get; set; }
        public float cost { get; set; }
    }
}
