using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using ApexBackend.Controllers;
using ApexBackend.Models;
using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApexBackend.Tests.Controllers
{
    [TestClass]
    public class DoctorsControllerTest
    {
        [TestMethod]
        public void ConfirmGetDoctorsReturnsListOfDoctors()
        {
            // arrange
            var fakeDbContext = A.Fake<ApplicationDbContext>();
            var fakeDbSet = A.Fake<DbSet<Doctor>>(options => options.Implements(typeof (IQueryable<Doctor>)));

            A.CallTo(() => fakeDbContext.Doctors).Returns(fakeDbSet);

            var repo = new DoctorsController(fakeDbContext);

            //act
            var doctors = repo.GetDoctors();

            //assert
            Assert.IsInstanceOfType(doctors, typeof (List<Doctor>));
        }

        [TestMethod]
        public void ConfirmGetDoctorReturnsHttpActionResultContainingDoctor()
        {
            // arrange
            var fakeDbContext = A.Fake<ApplicationDbContext>();
            var fakeDbSet = A.Fake<DbSet<Doctor>>(options => options.Implements(typeof(IQueryable<Doctor>)));

            A.CallTo(() => fakeDbContext.Doctors).Returns(fakeDbSet);

            var repo = new DoctorsController(fakeDbContext);

            //act
            var doctor = repo.GetDoctor(1);

            //assert
            Assert.IsInstanceOfType(doctor, typeof(OkNegotiatedContentResult<Doctor>));
        }
    }
}