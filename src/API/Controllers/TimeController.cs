// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimeController.cs" company="https://martincostello.com/">
//   Martin Costello (c) 2016
// </copyright>
// <summary>
//   TimeController.cs
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MartinCostello.Api.Controllers
{
    using System;
    using System.Globalization;
    using Microsoft.AspNet.Mvc;
    using Models;

    /// <summary>
    /// A class representing the controller for the <c>/api/time</c> resource.
    /// </summary>
    [Route("api/[controller]")]
    public class TimeController : Controller
    {
        /// <summary>
        /// Gets the current UTC time.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the current time.
        /// </returns>
        [HttpGet]
        public IActionResult Get()
        {
            var formatProvider = CultureInfo.InvariantCulture;
            var now = DateTimeOffset.UtcNow;

            var value = new TimeResponse()
            {
                Rfc1123 = now.ToString("r", formatProvider),
                UniversalFull = now.UtcDateTime.ToString("U", formatProvider),
                UniversalSortable = now.UtcDateTime.ToString("u", formatProvider),
                Unix = now.ToUnixTimeSeconds(),
            };

            return new ObjectResult(value);
        }
    }
}
