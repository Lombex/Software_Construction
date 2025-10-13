using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpAPI.Models
{
    public class Model_Payments
    {
        public string? transactions {  get; set; }
        public float amount { get; set; }
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
