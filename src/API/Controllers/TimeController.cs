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
    using System.Globalization;
    using System.Net;
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using NodaTime;
    using Swashbuckle.SwaggerGen.Annotations;

    /// <summary>
    /// A class representing the controller for the <c>/time</c> resource.
    /// </summary>
    [Route("time")]
    [Produces("application/json")]
    public class TimeController : Controller
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimeController"/> class.
        /// </summary>
        /// <param name="clock">The <see cref="IClock"/> to use.</param>
        public TimeController(IClock clock)
        {
            Clock = clock;
        }

        /// <summary>
        /// Gets the <see cref="IClock"/> in use by the instance.
        /// </summary>
        protected IClock Clock { get; }

        /// <summary>
        /// Gets the current UTC time.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the current time.
        /// </returns>
        [HttpGet]
        [Produces("application/json", Type = typeof(TimeResponse))]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(TimeResponse))]
        public IActionResult Get()
        {
            var formatProvider = CultureInfo.InvariantCulture;
            var now = Clock.GetCurrentInstant().ToDateTimeOffset();

            var value = new TimeResponse()
            {
                Timestamp = now,
                Rfc1123 = now.ToString("r", formatProvider),
                UniversalFull = now.UtcDateTime.ToString("U", formatProvider),
                UniversalSortable = now.UtcDateTime.ToString("u", formatProvider),
                Unix = now.ToUnixTimeSeconds(),
            };

            return new ObjectResult(value);
        }
    }
}
