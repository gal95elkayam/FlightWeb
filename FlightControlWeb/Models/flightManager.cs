using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb.Models
{
    public class FlightManager : IFlightManager
    {

        private static List<Flight> flights = new List<Flight>()
        {
            new Flight { flight_id ="216" , longitude=33.244, latitude=31.12, passengers=216, company_name=  "SwissAir", date_time= "2021-12-26T20:56:21Z",is_external=false },
            new Flight { flight_id ="217" , longitude=95.244, latitude=37.12, passengers=217, company_name=  "galsAir", date_time= "2024-12-26T23:56:21Z",is_external=false },
            new Flight { flight_id ="218" , longitude=73.244, latitude=98.12, passengers=216, company_name=  "SgggsAir", date_time= "2027-12-26T23:56:21Z",is_external=false }

            };

        public Location getInitialLocation(FlightPlan flight, DBContext context)
        {
            List<Location> locations = context.Locations.ToList();
            foreach (Location l in locations)
            {
                if (l.id == flight.id)
                {
                    return l;
                }
            }
            return null;
        }

        public async Task<bool> checkIfCurrAsync(DateTime relativeDate, FlightPlan flightPlan, DBContext _context)
        {
            int secondsForFlight = await calcSecOfFlightAsync(flightPlan, _context);
            DateTime flightBeginDate = DateTime.Parse(getInitialLocation(flightPlan, _context).date_time);
            DateTime flightEndDate = flightBeginDate.AddSeconds(secondsForFlight);

            // check if the flight is now:
            // if begin < relative: res is -
            int beginCompRel = DateTime.Compare(flightBeginDate, relativeDate);
            // if relative < end: res is -
            int endCompRel = DateTime.Compare(relativeDate, flightEndDate);
            if (beginCompRel <= 0 && endCompRel <= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<int> calcSecOfFlightAsync(FlightPlan flightPlan, DBContext context)
        {
            int time = 0;
            List<Segment> segment = await context.Segments.ToListAsync();
            foreach (Segment s in segment)
            {
                if (s.id == flightPlan.id)
                {
                    time += s.timespan_seconds;
                }
            }
            return time;
        }


        public Flight planToFlight(FlightPlan flightPlan, DBContext context)
        {
            Flight flightFromPlan = new Flight();
            flightFromPlan.flight_id = flightPlan.id;
            flightFromPlan.longitude = getInitialLocation(flightPlan, context).Longitude;
            flightFromPlan.latitude = getInitialLocation(flightPlan, context).Latitude;
            flightFromPlan.passengers = flightPlan.passengers;
            flightFromPlan.company_name = flightPlan.company_name;
            flightFromPlan.date_time = getInitialLocation(flightPlan, context).date_time;
            flightFromPlan.is_external = flightPlan.is_external;
            return flightFromPlan;
        }

     

        public void AddFlight(FlightPlan flightplan)
        {

            throw new NotImplementedException();
        }

        public void AddFlight(Flight flightplan)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Flight> GetAllFlight()
        {
            return flights;
        }

        public FlightPlan GetFlightById(object key)
        {
            throw new NotImplementedException();
        }

        public void Remove(object key)
        {
            throw new NotImplementedException();
        }

        public void UpdateFlight(object key)
        {
            throw new NotImplementedException();
        }
    }

}
