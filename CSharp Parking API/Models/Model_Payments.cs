using Microsoft.EntityFrameworkCore;

namespace CSharpAPI.Models
{
    public class M_Payments
    {
        // voeg een id toe als primaire sleutel
        public Guid id { get; set; }

        // andere velden
        public Guid reservation_id { get; set; }
        public float amount { get; set; }
        public DateTime paid_at { get; set; }

        public string? transactions {  get; set; }
        public string? initiator { get; set; }
        public DateTime created_at { get; set; }
        public DateTime completed { get; set; }
        public Guid hash { get; set; }
        public T_Data? t_data { get; set; }
        public Guid session_id { get; set; }
        public Guid parking_lot_id { get; set; }
    }

    [Owned]
    public class T_Data
    {
        public float amount { get; set; }
        public DateTime date { get; set; }
        public string? method { get; set; }
        public string? issuer { get; set; }
        public string? bank { get; set; }
    }
}
