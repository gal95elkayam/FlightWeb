using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb.Models
{
    public interface IFlightManager
    {
        void AddFlight(FlightPlan flightplan);
        IEnumerable<Flight> GetAllFlight();
        FlightPlan GetFlightById(object key);
        void Remove(object key);
        void UpdateFlight(object key);

    }
}
