using System.ComponentModel.DataAnnotations;

namespace CSharpAPI.Models
{
    public class M_Parkinglots
    {
        [Key]
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
    [Owned]
    public class Coordinates
    {
        public float lat { get; set; }
        public float lng { get; set; }

    }

    [Owned]
      public class M_Tariff
    {
        public Guid id { get; set; }
        public Guid parkinglotId { get; set; }
        public List<Tariff> tariffs { get; set; }
    }

    public class M_ParkinglotRates
    {
        public Guid id { get; set; }
        public Guid parkinglotId { get; set; }
        public List<M_Tariff> tariffs { get; set; }
    }


}

