using System.ComponentModel.DataAnnotations;

namespace CSharpAPI.Models
{
    // Model for hotels
    public class M_Hotel
    {
        [Key]
        public Guid id { get; set; }
        public string name { get; set; } = string.Empty; // Hotel name
        public string? address { get; set; } // Hotel address
        public string? phone { get; set; } // Hotel phone
        public string? email { get; set; } // Hotel email
        public decimal discount_percentage { get; set; } = 0; // Discount percentage for hotel guests (0-100)
        public bool active { get; set; } = true; // Whether hotel is active
        public DateTime created_at { get; set; } // When hotel was created
    }

    // Model for linking users to hotels (hotel guests)
    public class M_HotelGuest
    {
        [Key]
        public Guid id { get; set; }
        public Guid hotel_id { get; set; } // Hotel ID
        public Guid user_id { get; set; } // Guest user ID
        public DateTime check_in { get; set; } // Check-in date
        public DateTime? check_out { get; set; } // Check-out date (null if still checked in)
        public string? reservation_number { get; set; } // Hotel reservation number
        public bool discount_applied { get; set; } = false; // Whether discount has been applied
        public DateTime created_at { get; set; } // When guest record was created
    }
}

