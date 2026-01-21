using System.ComponentModel.DataAnnotations;

namespace CSharpAPI.Models
{
    // Model for billing/invoice records
    public class M_Billing
    {
        [Key]
        public Guid id { get; set; }
        public Guid user_id { get; set; } // User who owes the bill
        public Guid? session_id { get; set; } // Optional: related parking session
        public Guid? reservation_id { get; set; } // Optional: related reservation
        public Guid? payment_id { get; set; } // Optional: related payment
        public decimal amount { get; set; } // Amount due
        public string currency { get; set; } = "EUR"; // Currency code
        public string? description { get; set; } // Description of the bill
        public DateTime due_date { get; set; } // When payment is due
        public bool paid { get; set; } // Whether the bill has been paid
        public DateTime created_at { get; set; } // When the bill was created
        public DateTime? paid_at { get; set; } // When the bill was paid
        public string? invoice_number { get; set; } // Unique invoice number
        public BillingType type { get; set; } // Type of billing
        public BillingStatus status { get; set; } // Status of the bill
    }

    // Types of billing
    public enum BillingType
    {
        ParkingSession,    // Bill for a parking session
        Reservation,       // Bill for a reservation
        MonthlyBundle,     // Monthly bundle invoice for companies
        Refund,            // Refund billing
        Other              // Other types
    }

    // Status of billing
    public enum BillingStatus
    {
        Pending,           // Bill created but not yet due
        Due,               // Bill is due for payment
        Paid,              // Bill has been paid
        Overdue,           // Bill is past due date
        Cancelled,         // Bill has been cancelled
        Refunded           // Bill has been refunded
    }
}

