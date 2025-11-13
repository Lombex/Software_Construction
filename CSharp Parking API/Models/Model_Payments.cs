using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSharpAPI.Models
{
    public class M_Payments
    {
        public Guid id { get; set; }
        public Guid reservation_id { get; set; }
        public DateTime paid_at { get; set; }
        public string? transactions { get; set; }
        public float amount { get; set; }
        public string? initiator { get; set; }
        public DateTime created_at { get; set; }
        public DateTime completed { get; set; }
        public Guid hash { get; set; }
        public T_Data? t_data { get; set; }

        [ForeignKey("session_id")]
        public Guid session_id { get; set; }
        [ForeignKey("parking_lot_id")]
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