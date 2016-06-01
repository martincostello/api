// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ErrorController.cs" company="https://martincostello.com/">
//   Martin Costello (c) 2016
// </copyright>
// <summary>
//   ErrorController.cs
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MartinCostello.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// A class representing the controller for the <c>/error</c> resource.
    /// </summary>
    public class ErrorController : Controller
    {
        /// <summary>
        /// Gets the view for the error page.
        /// </summary>
        /// <param name="id">The optional HTTP status code associated with the error.</param>
        /// <returns>
        /// The view for the error page.
        /// </returns>
        [HttpGet]
        public IActionResult Index(int? id) => View("Error", id ?? 500);
    }
}
