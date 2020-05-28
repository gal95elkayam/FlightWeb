using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;



namespace UnitTestProject
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
        //[System.Obsolete]
        public async Task TestGetPlightPlan()
        {
            // Arrange
            FlightPlan fStub = new FlightPlan();
            fStub.company_name = "ELAL";
            fStub.passengers = 200;
            fStub.Initial_location.Longitude = 40;
            fStub.Initial_location.Latitude = 30;
            fStub.Initial_location.date_time = "2020-05-27T22:22:22Z";
            Segment seg1 = new Segment();
            seg1.Longitude = 41;
            seg1.Latitude = 31;
            seg1.timespan_seconds = 600;
            fStub.Segments.Add(seg1);

            // Act
            ActionResult<FlightPlan> post = await systemToTest.PostFlightPlan(fStub);
            ActionResult<FlightPlan> get = await systemToTest.GetFlightPlan(post.Value.id);
            // Assert
            Assert.Equals(post, get);
        }
    }
}


//namespace UnitTestProject
//{
//    [TestClass]
//    public class FlightsControllerTest
//    {
//        private readonly FlightsController systemToTest;
//        private readonly DBContext DBContextTest;

//        private async Task<ActionResult<FlightPlan>> createFlightPlan()
//        {
//            FlightPlan fStub = new FlightPlan();
//            fStub.company_name = "ELAL";
//            fStub.passengers = 200;
//            fStub.Initial_location.Longitude = 40;
//            fStub.Initial_location.Latitude = 30;
//            fStub.Initial_location.date_time = "2020-05-27T22:22:22Z";
//            Segment seg1 = new Segment();
//            seg1.Longitude = 41;
//            seg1.Latitude = 31;
//            seg1.timespan_seconds = 600;
//            fStub.Segments.Add(seg1);
//            return await fStub);
//        }

//        [TestMethod]
//        [System.Obsolete]
//        public void TestMethod1()
//        {
//            var f_id = "ABab12";
//            var longi = 30;
//            var lat = 40;
//            var pass = 200;
//            var comp_name = "ELAL";
//            var time = "2020-12-26T23:56:21Z";
//            var is_ext = false;
//            Flight newFlight = new Flight
//            {
//                flight_id = f_id,
//                longitude = longi,
//                latitude = lat,
//                passengers = pass,
//                company_name = comp_name,
//                date_time = time,
//                is_external = is_ext
//            };

//            var mock = new Mock<FlightPlanController>();
//            mock.Setup(x => x.GetFlightPlan(f_id)).Returns(createFlightPlan());

//            //Moq.Language.Flow.IReturnsResult<FlightPlanController> returnsResult =;

//        }


//    }
//}

