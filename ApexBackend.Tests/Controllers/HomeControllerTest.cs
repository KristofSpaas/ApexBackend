using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ApexBackend;
using ApexBackend.Controllers;

namespace ApexBackend.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTest
    {
        [TestMethod]
        public void ConfirmHomeControllerReturnsViewResult()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Home Page", result.ViewBag.Title);
        }
    }
}