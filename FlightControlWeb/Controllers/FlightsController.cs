using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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

        //// GET: api/Flights
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Flight>>> GetFlights()
        //{
        //    return await _context.Flights.ToListAsync();
        //}

        //// GET: api/Flights/5
        //[HttpGet("{id}")]
        //public async Task<ActionResult<Flight>> GetFlight(string id)
        //{
        //    var flight = await _context.Flights.FindAsync(id);

        //    if (flight == null)
        //    {
        //        return NotFound();
        //    }

        //    return flight;
        //}


        [HttpGet]
        [Obsolete]
        public async Task<ActionResult<IEnumerable<Flight>>> GetFlight([FromQuery] string relative_to)
        {
            if (relative_to == null)
            {
                return flightManager.flights ; /////check what to do!!!!!!!!!!!!
            }
            DateTime relativeDate = DateTime.Parse(relative_to.Substring(0,20));
            List<FlightPlan> flightsList = await _context.FlightPlan.ToListAsync();
            List<Server> externalServers = await _context.Servers.ToListAsync();
            var resultList = new List<Flight>();
            foreach (FlightPlan flightPlan in flightsList)
            {
                resultList.Add(await flightManager.FromInternal(relativeDate, flightPlan, _context));
                //if (relative_to.Contains("&sync_all"))
                //{
                //    resultList.Add(await flightManager.FromInternalAndExternal(relativeDate, flightPlan, _context));
                //} else
                //{
                //    resultList.Add(await flightManager.FromInternal(relativeDate, flightPlan, _context));
                //}
            }
            // from here!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //if (relative_to.Contains("&sync_all"))
            //{
            //    foreach(Server s in externalServers)
            //    {
            //        s.addExternalFlights(resultList, relativeDate);
            //        ///////// here

            //        resultList.Add(await flightManager.FromExternal(relativeDate, flightPlan, _context));
            //    }
                
            //}
            return resultList;
        }



        // PUT: api/Flights/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFlight(string id, Flight flight)
        {
            if (id != flight.flight_id)
            {
                return BadRequest();
            }

            _context.Entry(flight).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FlightExists(id))
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

        // POST: api/Flights
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Flight>> PostFlight(Flight flight)
        {
            _context.Flights.Add(flight);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFlight", new { id = flight.flight_id }, flight);
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
