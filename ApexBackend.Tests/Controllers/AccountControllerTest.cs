using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ApexBackend.Controllers;
using ApexBackend.Models;
using FakeItEasy;

namespace ApexBackend.Tests.Controllers
{
    [TestClass]
    public class AccountControllerTest
    {
        [TestMethod]
        public void ConfirmGetUsersReturnsListOfUsers()
        {
            // arrange
            var fakeDbContext = A.Fake<ApplicationDbContext>();
            var fakeDbSet =
                A.Fake<DbSet<ApplicationUser>>(options => options.Implements(typeof (IQueryable<ApplicationUser>)));

            A.CallTo(() => fakeDbContext.Users).Returns(fakeDbSet);

            var repo = new AccountController(fakeDbContext);

            //act
            var users = repo.GetUsers();

            //assert
            Assert.IsInstanceOfType(users, typeof (List<ApplicationUser>));
        }
    }
}