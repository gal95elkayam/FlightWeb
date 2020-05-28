
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlightControlWeb.Models;
using Microsoft.AspNetCore.Routing.Constraints;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace FlightControlWeb.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class FlightPlanController : ControllerBase
    {
        private readonly DBContext _context;
        private IMemoryCache cache;
        public FlightPlanController(DBContext context)
        {
            _context = context;
        }

        // GET: api/FlightPlan
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FlightPlan>>> GetFlightPlan(object value)
        {
            List<FlightPlan> list = await _context.FlightPlan.ToListAsync();
            // insert all the location's data and segments's data
            foreach (FlightPlan flight in list)
            {
                string tempId = flight.id;
                List<Location> locationsList = await _context.Locations.ToListAsync();
                List<Segment> segmentsList = await _context.Segments.ToListAsync();
                //get the location and the segments according to the id
                Location thisLocation = locationsList.Where(a => a.id == tempId).First();
                List<Segment> thisSegments = segmentsList.Where(a => a.id == tempId).ToList();

                flight.Segments = thisSegments;
                flight.Initial_location = thisLocation;
            }
            return await _context.FlightPlan.ToListAsync();

        }

        public bool beginWith(string a, string begining)
        {
            string beg = a.Substring(0, 6);
            if (string.Compare(a.Substring(0, 6), begining) == 0)
            {
                return true;
            } else
            {
                return false;
            }
        }

        public FlightPlan getFromExternalServer(string serverUrl, string flightId)
        {
            string url = serverUrl;
            url = string.Concat(url, "/api/FlightPlan");
            url = string.Concat(url, "/");
            url = string.Concat(url, flightId);
            string urlPath = string.Format(url);
            WebRequest requestObjGet = WebRequest.Create(urlPath);
            requestObjGet.Method = "GET";
            HttpWebResponse responseObjGet = null;
            responseObjGet = (HttpWebResponse)requestObjGet.GetResponse();
            string strRes = null;
            FlightPlan flightPlan = null;
            using (Stream stream = responseObjGet.GetResponseStream())
            {
                StreamReader sr = new StreamReader(stream);
                strRes = sr.ReadToEnd();
                sr.Close();
            }
            flightPlan = JsonConvert.DeserializeObject<FlightPlan>(strRes);
            flightPlan.is_external = true;
            return flightPlan;
        }


        // GET: api/FlightPlan/5
        [HttpGet("{id}")]
        public async Task<ActionResult<FlightPlan>> GetFlightPlan(string id)
        {
            // check if this is id of internal flight
            var flightPlan = await _context.FlightPlan.FindAsync(id);

            // the id doesnt exist or this is id of external flight
            if (flightPlan == null)
            {
                // check if this is id of external flight
                List<ExternalFlights> externalFlights = await _context.flightToServer.ToListAsync();
                // if the id exist in external server - ask the eternal server
                ExternalFlights ef = _context.flightToServer.Find(id);
                if (ef != null)
                {
                    return getFromExternalServer(ef.serverUrl, ef.flightId);
                }
                return NotFound();
            }
            else
            {
                string tempId = flightPlan.id;
                List<Location> locationsList = await _context.Locations.ToListAsync();
                List<Segment> segmentsList = await _context.Segments.ToListAsync();
                //get the location and the segments according to the id
                Location thisLocation = locationsList.Where(a => a.id == tempId).First();
                List<Segment> thisSegments = segmentsList.Where(a => beginWith(a.id, tempId) == true).ToList();

                flightPlan.Segments = thisSegments;
                flightPlan.Initial_location = thisLocation;
                return flightPlan;
            }
        }

        // PUT: api/FlightPlan/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFlightPlan(string id, FlightPlan flightPlan)
        {
            if (id != flightPlan.id)
            {
                return BadRequest();
            }

            _context.Entry(flightPlan).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FlightPlanExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        public string createRandomId()
        {
            var charsBig = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var charsSmall = "abcdefghijklmnopqrstuvwxyz";
            var nums = "0123456789";
            var stringChars = new char[6];
            var random = new Random();
            stringChars[0] = charsBig[random.Next(charsBig.Length)];
            stringChars[1] = charsBig[random.Next(charsBig.Length)];
            stringChars[2] = charsSmall[random.Next(charsBig.Length)];
            stringChars[3] = charsSmall[random.Next(charsSmall.Length)];
            stringChars[4] = nums[random.Next(nums.Length)];
            stringChars[5] = nums[random.Next(nums.Length)];
            return new String(stringChars);
        }

        private bool thereIsAInvaldSegment(List<Segment> segmentList)
        {
            foreach (Segment s in segmentList)
            {
                if (s.Longitude < -180 || s.Longitude > 180 || s.Latitude < -90 || s.Latitude > 90)
                {
                    return true;
                }
            }
            return false;
        }


        //static string Id = "100000"; //change to another id!
        //static string Id = "100000"; //change to another id!
        // POST: api/FlightPlan
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<FlightPlan>> PostFlightPlan(FlightPlan flightPlan)
        {
            if (flightPlan.company_name == null || flightPlan.Segments == null || flightPlan.Initial_location == null
                || flightPlan.passengers <= 0 || flightPlan.Initial_location.Latitude < -90 || flightPlan.Initial_location.Latitude >90
                || flightPlan.Initial_location.Longitude < -180 || flightPlan.Initial_location.Longitude > 180 ||
                thereIsAInvaldSegment(flightPlan.Segments))
            {
                Response.StatusCode = 500;  
                return Content("Invalid data");
                //return BadRequest();
            }
            flightPlan.is_external = false; 
            flightPlan.id = createRandomId();
            //int tempId = Int32.Parse(Id);   
            var segmentList = flightPlan.Segments;
            int i = 0;
            foreach (Segment s in segmentList)
            {
                s.id = string.Concat(flightPlan.id, i.ToString());
                i++;
                // insert the segments to the DB
                _context.Segments.Add(s);
            }

            flightPlan.Initial_location.id = flightPlan.id;
            // insert the location to the DB of the locations.
            _context.FlightPlan.Add(flightPlan);
            await _context.SaveChangesAsync();
           // tempId++;
            //Id = tempId.ToString();
            return CreatedAtAction("GetFlightPlan", new { id = flightPlan.id }, flightPlan);
        }

        // DELETE: api/FlightPlan/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<FlightPlan>> DeleteFlightPlan(string id)
        {
            Console.WriteLine("in delete\n");
            var flightPlan = await _context.FlightPlan.FindAsync(id);
            if (flightPlan == null)
            {
                return NotFound();
            }

            _context.FlightPlan.Remove(flightPlan);
            await _context.SaveChangesAsync();

            return flightPlan;
        }

        private bool FlightPlanExists(string id)
        {
            return _context.FlightPlan.Any(e => e.id == id);
        }

    }

}
