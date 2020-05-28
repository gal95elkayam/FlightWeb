using Microsoft.VisualStudio.TestTools.UnitTesting;
using FlightControlWeb.Controllers;
using FlightControlWeb.Models;

using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using FlightControlWeb;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace UnitTestProject3
{
    [TestClass]
    public class FlightsControllerTest
    {
        private readonly FlightPlanController systemToTest;
        private readonly DBContext DBContextTest;
        //[TestMethod()]
        //public async Task ConfigureServicesTest1()
        //{
        //    // Build the data base.
        //    dataBase = new SqliteDataBase("sqliteTest.sqlite");
        //    await CreateTest();
        //}


        public FlightsControllerTest()
        {
            string[] args = { };
            var host = Program.CreateHostBuilder(args);
            var db = new DbContextOptionsBuilder<DBContext>();
            db.UseInMemoryDatabase("DBName");
            var dbOptions = db.Options;
            DBContextTest = new DBContext(dbOptions);
            systemToTest = new FlightPlanController(DBContextTest);
        }

        [TestMethod]
        public async Task TestGetPlightPlan()
        {
            // Arrange
            FlightPlan fStub = new FlightPlan();
            fStub.company_name = "ELAL";
            fStub.passengers = 200;

            Location loc = new Location();
            loc.Longitude = 40;
            loc.Latitude = 30;
            loc.date_time = "2020-05-27T22:22:22Z";
            fStub.Initial_location = loc;
            List<Segment> listOfSeg =  new List<Segment>();
            fStub.Segments = listOfSeg;

            // Act
            ActionResult<FlightPlan> post = await systemToTest.PostFlightPlan(fStub);
            ActionResult<IEnumerable<FlightPlan>> get = await systemToTest.GetFlightPlan((post.Result as CreatedAtActionResult).Value);
            // Assert
            Assert.IsTrue(((FlightPlan)(post.Result as CreatedAtActionResult).Value) == get.Value.ToList()[0]);
        }

        [TestMethod]
        public async Task TestGetPlightPlanWithNull()
        {
            // Arrange
            FlightPlan fStub = new FlightPlan();
            fStub.company_name = null;
            fStub.passengers = 200;

            Location loc = new Location();
            loc.Longitude = 40;
            loc.Latitude = 30;
            loc.date_time = "2020-05-27T22:22:22Z";
            fStub.Initial_location = loc;
            List<Segment> listOfSeg = new List<Segment>();
            fStub.Segments = listOfSeg;

            // Act
            ActionResult<FlightPlan> post = await systemToTest.PostFlightPlan(fStub);
            ActionResult<IEnumerable<FlightPlan>> get = await systemToTest.GetFlightPlan((post.Result as CreatedAtActionResult).Value);
            // Assert
            Assert.IsTrue(((FlightPlan)(post.Result as CreatedAtActionResult).Value) == get.Value.ToList()[0]);
        }
    }
}
