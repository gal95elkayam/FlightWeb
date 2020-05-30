using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;


namespace FlightControlWeb.Models
{
    public class FlightManager
    {

        // Return the initial location of the filght
        private Location getInitialLocation(FlightPlan flight, DBContext context)
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


        // return true if the flight fly now
        private async Task<bool> checkIfCurrAsync(DateTime relativeDate,
            FlightPlan flightPlan, DBContext _context)
        {
            int secondsForFlight = await calcSecOfFlightAsync(flightPlan, _context);
            DateTime flightBeginDate = TimeZoneInfo.ConvertTimeToUtc
                (DateTime.Parse(getInitialLocation(flightPlan, _context).date_time));
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


        // return the flight if it is flying now from internal server
        public async Task<Flight> fromInternal(DateTime relativeDate
            , FlightPlan flightPlan, DBContext _context)
        {
            if (flightPlan.is_external == false &&
                await checkIfCurrAsync(relativeDate, flightPlan, _context))
            {
                Flight flightToInsert = planToFlight(flightPlan, _context, relativeDate);
                flightToInsert.is_external = false;
                return flightToInsert;
            }
            return null;
        }


        // convert the month/ day to 2 chars
        private string toTwoCharString(string toString)
        {
            if (toString.Length == 1)
            {
                return string.Concat("0", toString);
            }
            return toString;
        }


        // Return list of external from specific server
        private List<Flight> getExtFromGetApi(Server s, DateTime relativeDate)
        {
            // create the URL
            string url = s.ServerURL;
            url = string.Concat(url, "/api/Flights?relative_to=");

            IEnumerable<string> list = new List<string>(){url,relativeDate.
                    Year.ToString(), "-", toTwoCharString(relativeDate.Month.ToString()),
                    "-", toTwoCharString(relativeDate.Day.ToString()), "T",
                        toTwoCharString(relativeDate.Hour.ToString()), ":",
                    toTwoCharString(relativeDate.Minute.ToString()) , ":",
                    toTwoCharString(relativeDate.Second.ToString()), "Z"};
            url = string.Concat(list);
            string urlPath = string.Format(url);

            WebRequest requestObjGet = WebRequest.Create(urlPath);
            requestObjGet.Method = "GET";
            HttpWebResponse responseObjGet = null;
            try
            {
                responseObjGet = (HttpWebResponse)requestObjGet.GetResponse();
            }
            catch (System.Net.WebException)
            {     
            }

            string strRes = null;
            using (Stream stream = responseObjGet.GetResponseStream())
            {
                StreamReader sr = new StreamReader(stream);
                strRes = sr.ReadToEnd();
                sr.Close();
            }
            List<Flight> listOfFlights = new List<Flight>();
            listOfFlights = JsonConvert.DeserializeObject<List<Flight>>(strRes);
            return listOfFlights;
        }


        // save the flight in DB
        private void saveExtFlightInDB(Flight f, Server s, DBContext _context)
        {
            f.is_external = true;
            // insert the flight to the map between 
            ExternalFlights ef = new ExternalFlights();
            ef.serverId = s.ServerId;
            ef.serverUrl = s.ServerURL;
            ef.flightId = f.flight_id;
            if (_context.flightToServer.Find(ef.flightId) == null)
            {
                _context.flightToServer.Add(ef);
                try
                {
                    _context.SaveChanges();
                }
                catch
                {
                }
            }
        }


        // return list of external flights that are flying now from all servers
        public async Task<List<Flight>> fromExternal(DateTime relativeDate, DBContext _context)
        {
            List<Server> externalServers = await _context.Servers.ToListAsync();
            List<Flight> resList = new List<Flight>();
            // get all flight from server s
            foreach (Server s in externalServers)
            {
                List<Flight> listOfFlights = new List<Flight>();
                listOfFlights = getExtFromGetApi(s, relativeDate);
                foreach (Flight f in listOfFlights)
                {
                    saveExtFlightInDB(f, s, _context);        
                }
                resList.AddRange(listOfFlights);
            }
            return resList;
        }


        // return true if a begin with b
        private bool beginWith(string a, string begining)
        {
            if (string.Compare(a.Substring(0, 6), begining) == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // calculate the time in seconds of flight
        private async Task<int> calcSecOfFlightAsync(FlightPlan flightPlan, DBContext context)
        {
            int time = 0;
            List<Segment> segment = await context.Segments.ToListAsync();
            foreach (Segment s in segment)
            {
                if (beginWith(s.id, flightPlan.id))
                {
                    time += s.timespan_seconds;
                }
            }
            return time;
        }

        // if this is the cuurent segment- change the location in flight plan
        private void checkIfCurrSegment(DateTime begin, DateTime end, double longBegin,
            double latBegin, int beginCompRel, int endCompRel, DateTime relativeDate,
             Flight flightFromPlan, Segment s)
        {
            if (beginCompRel <= 0 && endCompRel <= 0)
            {
                double relativeTimePassed =
                    (relativeDate - begin).TotalSeconds / (end - begin).TotalSeconds;
                flightFromPlan.longitude =
                    longBegin + relativeTimePassed * (s.Longitude - longBegin);
                flightFromPlan.latitude =
                    latBegin + relativeTimePassed * (s.Latitude - latBegin);
            }
        }


        // find the current location of the flight
        private async void findCurrLongAndLat(FlightPlan flightPlan, DBContext context
            , Flight flightFromPlan, DateTime relativeDate)
        {
            double longBegin = flightPlan.Initial_location.Longitude;
            double latBegin = flightPlan.Initial_location.Latitude;
            List<Segment> segment = await context.Segments.ToListAsync();
            DateTime begin;
            DateTime end = TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse
                (getInitialLocation(flightPlan, context).date_time));
            foreach (Segment s in segment)
            {
                if (beginWith(s.id, flightPlan.id))
                {
                    begin = end;
                    end = begin.AddSeconds(s.timespan_seconds);
                    //check if the flight is now:
                    //if begin < relative: res is -
                    int beginCompRel = DateTime.Compare(begin, relativeDate);
                    //if relative < end: res is -
                    int endCompRel = DateTime.Compare(relativeDate, end);
                    checkIfCurrSegment(begin, end, longBegin, latBegin, beginCompRel, endCompRel, 
                        relativeDate, flightFromPlan, s);
                }
                longBegin = s.Longitude;
                latBegin = s.Latitude;
            }

        }

        // convert fligth plan to flight object
        private Flight planToFlight(FlightPlan flightPlan,DBContext context,DateTime relativeDate)
        {
            Flight flightFromPlan = new Flight();
            flightFromPlan.flight_id = flightPlan.id;
            findCurrLongAndLat(flightPlan, context, flightFromPlan, relativeDate);
            flightFromPlan.passengers = flightPlan.passengers;
            flightFromPlan.company_name = flightPlan.company_name;
            flightFromPlan.date_time = getInitialLocation(flightPlan, context).date_time;
            return flightFromPlan;
        }
    }

}