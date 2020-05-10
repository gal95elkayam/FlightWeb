using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb.Models
{
    public interface IFlightManager
    {
        public void AddFlight(FlightPlan flightplan);
        public IEnumerable<Flight> GetAllFlight();
        public FlightPlan GetFlightById(object key);
        public void Remove(object key);
        public void UpdateFlight(object key);

    }
}
