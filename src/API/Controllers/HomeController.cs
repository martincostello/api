// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HomeController.cs" company="https://martincostello.com/">
//   Martin Costello (c) 2016
// </copyright>
// <summary>
//   HomeController.cs
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MartinCostello.Api.Controllers
{
    using Microsoft.AspNet.Mvc;

    /// <summary>
    /// A class representing the controller for the <c>/</c> resource.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Gets the view for the error page.
        /// </summary>
        /// <returns>
        /// The view for the error page.
        /// </returns>
        [HttpGet]
        public IActionResult Error() => View();

        /// <summary>
        /// Gets the view for the home page.
        /// </summary>
        /// <returns>
        /// The view for the home page.
        /// </returns>
        [HttpGet]
        public IActionResult Index() => View();
    }
}
