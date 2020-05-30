using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlightControlWeb.Models;

namespace FlightControlWeb.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class FlightsController : ControllerBase
    {
        private readonly DBContext _context;
        private FlightManager flightManager = new FlightManager();

        public FlightsController(DBContext context)
        {
            _context = context;
        }

        // Returns true if the relative time is valid
        private bool isValidRelativeTime(string relative_to)
        {
            if (relative_to.Length != 20)
            {
                return false;
            }
            try
            {
                DateTime relativeDate = TimeZoneInfo.
                ConvertTimeToUtc(DateTime.Parse(relative_to.Substring(0, 20)));
            }
            catch
            {
                return false;
            }
            return true;
        }

        // get flight according to relative time
        [HttpGet]
        [Obsolete]
        public virtual async Task<ActionResult<IEnumerable<Flight>>> 
            GetFlight([FromQuery] string relative_to)
        {
            string urlRequest = Request.QueryString.Value;
            // if there is no relative_to
            if (relative_to == null || !isValidRelativeTime(relative_to))
            {
               return BadRequest();
            }
            
            DateTime relativeDate = TimeZoneInfo.
                ConvertTimeToUtc(DateTime.Parse(relative_to.Substring(0,20)));
            List<FlightPlan> flightsList = await _context.FlightPlan.ToListAsync();

            List<Flight> resultList = new List<Flight>();
            // add to list every internal flight that is flying now
            foreach (FlightPlan flightPlan in flightsList)
            {
                Flight toAdd=await flightManager.fromInternal(relativeDate, flightPlan, _context);
                if (toAdd != null)
                {
                    resultList.Add(toAdd);
                }
                  
            }
            // add to list every external flight that is flying now
            if (urlRequest.Contains("&sync_all"))
            {
                var fromExt = await flightManager.fromExternal(relativeDate, _context);
                foreach (Flight f in fromExt)
                {
                    resultList.Add(f);
                }
            }
            return resultList;
        }


        // DELETE: api/Flights/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<FlightPlan>> DeleteFlight(string id)
        {
            // delete the flightPlan that connected to this flight
            var flightPlan = await _context.FlightPlan.FindAsync(id);
            if (flightPlan == null)
            {
                return NotFound();
            }

            //remove the flight from DB
            _context.FlightPlan.Remove(flightPlan);
            await _context.SaveChangesAsync();

            return flightPlan;
        }

        private bool FlightExists(string id)
        {
            return _context.Flights.Any(e => e.flight_id == id);
        }
    }
}