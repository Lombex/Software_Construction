namespace CSharpAPI.Models
{
    public class M_Session
    {
        public enum PaymentStatus
        {
            Paid,
            Unpaid
        }

        public Guid id { get; set; }
        public Guid parking_lot_id { get; set; }
        public string? license_plate { get; set; }
        public DateTime started { get; set; }
        public DateTime stopped { get; set; }
        public string? user { get; set; }
        public int duration_minutes { get; set; }
        public float cost { get; set; }
        public PaymentStatus status { get; set; }
    }
}
