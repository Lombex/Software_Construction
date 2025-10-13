using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp_Parking_API.Models
{
    public class Model_Parking_lots
    {
        public Guid id { get; set; }
        public string? name { get; set; }
        public string? location { get; set; }
        public string? address { get; set; }
        public int capacity { get; set; }
        public int reserved { get; set; }
        public float daytarriff { get; set; }
        public DateTime created_at { get; set; }
        public Coordinates? coordinates { get; set; }
    }

    public class Coordinates
    {
        public float lat { get; set; }
        public float lng { get; set; }

    }
}
