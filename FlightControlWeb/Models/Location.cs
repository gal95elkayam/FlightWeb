﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb.Models
{
    public class Location
    {
        public string id { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string date_time { get; set; }
    }
}
