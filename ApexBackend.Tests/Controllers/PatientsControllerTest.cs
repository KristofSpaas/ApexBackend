﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using ApexBackend.Controllers;
using ApexBackend.Models;
using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Http.Results;

namespace ApexBackend.Tests.Controllers
{
    [TestClass]
    public class PatientsControllerTest
    {
        //[TestMethod]
        //public void ConfirmGetPatientsReturnsListOfPatients()
        //{
        //    // arrange
        //    var fakeDbContext = A.Fake<ApplicationDbContext>();
        //    var fakeDbSet = A.Fake<DbSet<Patient>>(options => options.Implements(typeof (IQueryable<Patient>)));

        //    A.CallTo(() => fakeDbContext.Patients).Returns(fakeDbSet);

        //    var repo = new PatientsController(fakeDbContext);

        //    //act
        //    var patients = repo.GetPatientsByDoctorId(1);

        //    //assert
        //    Assert.IsInstanceOfType(patients, typeof(OkNegotiatedContentResult<List<Patient>>));
        //}
    }
}