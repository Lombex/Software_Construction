using System.ComponentModel.DataAnnotations;

namespace CSharpAPI.Models
{
    public class M_Users
    {
        // Three-tier role system for parking management
        public enum UserRole
        {
            ParkingUser,        // Regular user - can book parking, view own reservations
            ParkingLotAdmin,    // Admin for specific parking lot - manages one lot
            SuperAdmin          // System administrator - full access to all lots and users
        }
        [Key]
        public Guid id { get; set; }
        public string? username { get; set; }
        public string? password { get; set; }
        public string? name { get; set; }
        public string? email { get; set; }
        public string? phone { get; set; }
        public UserRole role { get; set; }
        public Guid? parking_lot_id { get; set; } // For ParkingLotAdmin - which lot they manage
        public DateTime created_at { get; set; }
        public DateTime birth_year { get; set; }
        public bool active { get; set; }
    }
}
