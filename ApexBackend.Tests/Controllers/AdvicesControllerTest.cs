using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Http.Results;
using ApexBackend.Controllers;
using ApexBackend.Models;
using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApexBackend.Tests.Controllers
{
    [TestClass]
    public class AdvicesControllerTest
    {
        [TestMethod]
        public void ConfirmGetMessageReturnsHttpActionResultContainingMessage()
        {
            // arrange
            var fakeDbContext = A.Fake<ApplicationDbContext>();
            var fakeDbSet = A.Fake<DbSet<Advice>>(options => options.Implements(typeof (IQueryable<Advice>)));

            A.CallTo(() => fakeDbContext.Advices).Returns(fakeDbSet);

            var repo = new AdvicesController(fakeDbContext);

            //act
            var advice = repo.GetAdvice(1);

            //assert
            Assert.IsInstanceOfType(advice, typeof (OkNegotiatedContentResult<Advice>));
        }
    }
}