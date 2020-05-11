using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlightControlWeb.Models;

namespace FlightControlWeb.Models
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions<DBContext> options)
            : base(options)
        {
        }

        public DbSet<Flight> Flights { get; set; }
        public DbSet<FlightPlan> FlightPlan { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Segment> Segments { get; set; }
        public DbSet<Server> Servers { get; set; }
    }
}
