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
    public class MessagesControllerTest
    {
        [TestMethod]
        public void ConfirmGetMessageReturnsHttpActionResultContainingMessage()
        {
            // arrange
            var fakeDbContext = A.Fake<ApplicationDbContext>();
            var fakeDbSet = A.Fake<DbSet<Message>>(options => options.Implements(typeof(IQueryable<Message>)));

            A.CallTo(() => fakeDbContext.Messages).Returns(fakeDbSet);

            var repo = new MessagesController(fakeDbContext);

            //act
            var message = repo.GetMessage(1);

            //assert
            Assert.IsInstanceOfType(message, typeof(OkNegotiatedContentResult<Message>));
        }
    }
}
