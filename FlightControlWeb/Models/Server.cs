using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb.Models
{
    public class Server
    {
        [System.ComponentModel.DataAnnotations.Key]
        public string ServerId { get; set; }
        public string ServerURL { get; set; }

        public void addExternalFlights(List<Flight> resultList, DateTime relativeDate)
        {

            var url = string.Concat(this.ServerURL, "/api/Flights?relative_to=", relativeDate,ToString());

        }
    }
}
