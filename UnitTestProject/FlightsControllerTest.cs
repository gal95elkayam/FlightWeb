using Microsoft.VisualStudio.TestTools.UnitTesting;
using FlightControlWeb.Controllers;
using FlightControlWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using FlightControlWeb;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace UnitTestProject
{
    [TestClass]
    public class FlightsControllerTest
    {
        private readonly FlightPlanController systemToTest;
        private readonly DBContext DBContextTest;

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
            ActionResult<IEnumerable<FlightPlan>> get = 
                await systemToTest.GetFlightPlan((post.Result as CreatedAtActionResult).Value);
            // Assert
            Assert.IsTrue(((FlightPlan)(post.Result as CreatedAtActionResult).Value) ==
                get.Value.ToList()[0]);
        }

        [TestMethod]
        public async Task TestGetPlightPlanWithNullCompany()
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

            ActionResult<FlightPlan> post;

            // act + assert
            try
            {
                post = await systemToTest.PostFlightPlan(fStub);
                Assert.Fail(); // raises AssertionException
            }
            catch 
            {
               
            }

        }

        [TestMethod]
        public async Task TestInvalidDateTime()
        {
            // Arrange
            FlightPlan fStub = new FlightPlan();
            fStub.company_name = "air";
            fStub.passengers = 200;

            Location loc = new Location();
            loc.Longitude = 40;
            loc.Latitude = 30;
            loc.date_time = "2020-5-27T22:22:22Z";
            fStub.Initial_location = loc;
            List<Segment> listOfSeg = new List<Segment>();
            fStub.Segments = listOfSeg;

            ActionResult<FlightPlan> post;

            // act + assert
            try
            {
                post = await systemToTest.PostFlightPlan(fStub);
                Assert.Fail(); // raises AssertionException
            }
            catch
            {

            }

        }

        [TestMethod]
        public async Task TestInvalidNumOfPass()
        {
            // Arrange
            FlightPlan fStub = new FlightPlan();
            fStub.company_name = "air";
            fStub.passengers = -200;

            Location loc = new Location();
            loc.Longitude = 40;
            loc.Latitude = 30;
            loc.date_time = "2020-05-27T22:22:22Z";
            fStub.Initial_location = loc;
            List<Segment> listOfSeg = new List<Segment>();
            fStub.Segments = listOfSeg;

            ActionResult<FlightPlan> post;

            // act + assert
            try
            {
                post = await systemToTest.PostFlightPlan(fStub);
                Assert.Fail(); // raises AssertionException
            }
            catch
            {

            }

        }

        [TestMethod]
        public async Task TestInvalidLocation()
        {
            // Arrange
            FlightPlan fStub = new FlightPlan();
            fStub.company_name = null;
            fStub.passengers = 200;

            Location loc = new Location();
            loc.Longitude = 240;
            loc.Latitude = 30;
            loc.date_time = "2020-05-27T22:22:22Z";
            fStub.Initial_location = loc;
            List<Segment> listOfSeg = new List<Segment>();
            fStub.Segments = listOfSeg;

            ActionResult<FlightPlan> post;

            // act + assert
            try
            {
                post = await systemToTest.PostFlightPlan(fStub);
                Assert.Fail(); // raises AssertionException
            }
            catch
            {

            }

        }
    }
}
