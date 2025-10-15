namespace CSharpAPI.Models
{
    public class M_Vehicles
    {
        public Guid id { get; set; }
        public Guid user_id { get; set; }
        public string? license_plate { get; set; }
        public string? make { get; set; }
        public string? model { get; set; }
        public string? color { get; set; }
        public DateTime year { get; set; }
        public DateTime created_at { get; set; }
    }
}
