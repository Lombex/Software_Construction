using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSharpAPI.Models
{
    public class M_Session
    {
        public enum PaymentStatus
        {
            Paid,
            Unpaid
        }
        [Key]
        public Guid id { get; set; }
        [ForeignKey(nameof(parking_lot_id))]
        public M_Parkinglots parking_lot { get; set; }
        public Guid parking_lot_id { get; set; }
        public Guid vehicle_id { get; set; }
        [ForeignKey(nameof(vehicle_id))]
        public M_Vehicles? vehicle_license_plate { get; set; }
        public string? license_plate { get; set; }
        public DateTime started { get; set; }
        public DateTime stopped { get; set; }
        public string? user { get; set; }
        public int duration_minutes { get; set; }
        public float cost { get; set; }
        public PaymentStatus status { get; set; }
    }
}
