using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using ApexBackend.Controllers;
using ApexBackend.Models;
using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApexBackend.Tests.Controllers
{
    [TestClass]
    public class PatientsControllerTest
    {
        [TestMethod]
        public void ConfirmGetPatientsReturnsListOfPatients()
        {
            // arrange
            var fakeDbContext = A.Fake<ApplicationDbContext>();
            var fakeDbSet = A.Fake<DbSet<Patient>>(options => options.Implements(typeof (IQueryable<Patient>)));

            A.CallTo(() => fakeDbContext.Patients).Returns(fakeDbSet);

            var repo = new PatientsController(fakeDbContext);

            //act
            var patients = repo.GetPatients();

            //assert
            Assert.IsInstanceOfType(patients, typeof (List<Patient>));
        }
    }
}