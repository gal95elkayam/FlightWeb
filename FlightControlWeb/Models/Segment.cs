using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb.Models
{
    public class Segment
    {
        [Key]
        public string id { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public int timespan_seconds { get; set; }
    }
}
