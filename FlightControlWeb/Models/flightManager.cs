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

        public List<Flight> flights = new List<Flight>()
        {
            
            new Flight { flight_id ="216" , longitude=33.244, latitude=31.12, passengers=216, company_name=  "SwissAir", date_time= "2021-12-26T20:56:21Z",is_external=false },
            new Flight { flight_id ="217" , longitude=95.244, latitude=37.12, passengers=217, company_name=  "galsAir", date_time= "2024-12-26T23:56:21Z",is_external=false },
            new Flight { flight_id ="218" , longitude=73.244, latitude=98.12, passengers=216, company_name=  "SgggsAir", date_time= "2027-12-26T23:56:21Z",is_external=false }

            };

        private object flightManager;

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
            DateTime flightBeginDate = TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(getInitialLocation(flightPlan, _context).date_time));
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

        public async Task<Flight> fromInternal(DateTime relativeDate, FlightPlan flightPlan, DBContext _context)
        {
            if (flightPlan.is_external == false && await checkIfCurrAsync(relativeDate, flightPlan, _context))
            {
                Flight flightToInsert = planToFlight(flightPlan, _context, relativeDate);
                flightToInsert.is_external = false;
                return flightToInsert;
            }
            return null;
        }

        string toTwoCharString(string toString)
        {
            if (toString.Length == 1)
            {
                return string.Concat("0", toString);
            }
            return toString;
        }
        
        private int calcNumOfFlights(string strRes)
        {
            if (!strRes.Contains("\"is_external\":false}"))
            {
                return 0;
            }
            string[] flights = strRes.Split("\"is_external\":false},");
            return flights.Length;
        }

        public async Task<List<Flight>> fromExternal(DateTime relativeDate, DBContext _context)
        { 
            List<Server> externalServers = await _context.Servers.ToListAsync();
            List<Flight> resList = new List<Flight>();
            // get all flight from server s
            foreach (Server s in externalServers)
            {

                string url = s.ServerURL;
                url = string.Concat(url, "/api/Flights?relative_to=");
               
                IEnumerable<string> list = new List<string>(){url,relativeDate.Year.ToString(), "-", toTwoCharString(relativeDate.Month.ToString()),
                    "-", toTwoCharString(relativeDate.Day.ToString()), "T",
                        toTwoCharString(relativeDate.Hour.ToString()), ":", toTwoCharString(relativeDate.Minute.ToString()) , ":", toTwoCharString(relativeDate.Second.ToString()), "Z"};
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
                    continue;
                }

                string strRes = null;
                //var listOfFlights;
                using (Stream stream = responseObjGet.GetResponseStream())
                {
                    StreamReader sr = new StreamReader(stream);
                    strRes = sr.ReadToEnd();
                    sr.Close();
                }
                int numOfFlights = calcNumOfFlights(strRes);
                List<Flight> listOfFlights = new List<Flight>();
                listOfFlights = JsonConvert.DeserializeObject<List<Flight>>(strRes);
                //try
                //{
                    
                //}
                //catch (System.Net.WebException)
                //{
                //    continue;
                //}

                foreach (Flight f in listOfFlights)
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
                        // _context.flightToServer.Remove(_context.flightToServer.Find(ef.flightId));
                        _context.SaveChanges();
                    }
                }
                
                resList.AddRange(listOfFlights);
            }
            return resList;
        }

       

        bool beginWith(string a, string begining)
        {
            string beg = a.Substring(0, 6);
            if (string.Compare(a.Substring(0, 6), begining) == 0)
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
                if (beginWith(s.id,flightPlan.id))
                {
                    time += s.timespan_seconds;
                }
            }
            return time;
        }

        private async void findCurrSeg(FlightPlan flightPlan, DBContext context, Flight flightFromPlan, DateTime relativeDate)
        {
            double longBegin = flightPlan.Initial_location.Longitude;
            double latBegin = flightPlan.Initial_location.Latitude;
            List<Segment> segment = await context.Segments.ToListAsync();
            DateTime begin;
            DateTime end = TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(getInitialLocation(flightPlan, context).date_time));
            foreach (Segment s in segment)
            {
                if (beginWith(s.id,flightPlan.id))
                {
                    begin = end;
                    end = begin.AddSeconds(s.timespan_seconds);
                    //check if the flight is now:
                    //if begin < relative: res is -
                    int beginCompRel = DateTime.Compare(begin, relativeDate);
                    //if relative < end: res is -
                    int endCompRel = DateTime.Compare(relativeDate, end);

                    if (beginCompRel <= 0 && endCompRel <= 0)
                    {
                        double relativeTimePassed = (relativeDate - begin).TotalSeconds / (end - begin).TotalSeconds;
                        flightFromPlan.longitude = longBegin + relativeTimePassed*(s.Longitude - longBegin);
                        flightFromPlan.latitude = latBegin + relativeTimePassed * (s.Latitude - latBegin);
                        break;
                    }
                }
                longBegin = s.Longitude;
                latBegin = s.Latitude;
            }
           
        }


        public Flight planToFlight(FlightPlan flightPlan, DBContext context, DateTime relativeDate)
        {
            Flight flightFromPlan = new Flight();
            flightFromPlan.flight_id = flightPlan.id;
            findCurrSeg(flightPlan, context, flightFromPlan, relativeDate);
            flightFromPlan.passengers = flightPlan.passengers;
            flightFromPlan.company_name = flightPlan.company_name;
            flightFromPlan.date_time = getInitialLocation(flightPlan, context).date_time; // change to the time now?
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

        public void 
            Flight(object key)
        {
            throw new NotImplementedException();
        }
    }

}
