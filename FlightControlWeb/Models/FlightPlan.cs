using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb.Models
{
    public class FlightPlan
    {
        public string id { get; set; }
        public int passengers { get; set; }
        public string company_name { get; set; }
        public Location Initial_location { get; set; }
        public bool is_external { get; set; }
        [NotMapped]
        public List<Segment> Segments { get; set; }
    }
}
