// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api.Controllers
{
    using System.Globalization;
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using NodaTime;
    using Swagger;
    using Swashbuckle.AspNetCore.Annotations;

    /// <summary>
    /// A class representing the controller for the <c>/time</c> resource.
    /// </summary>
    [ApiController]
    [Route("time")]
    [Produces("application/json")]
    public class TimeController : ControllerBase
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
        [EnableCors(Startup.DefaultCorsPolicyName)]
        [HttpGet]
        [Produces("application/json", Type = typeof(TimeResponse))]
        [ProducesResponseType(typeof(TimeResponse), StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(TimeResponse), Description = "The current UTC date and time.")]
        [SwaggerResponseExample(typeof(TimeResponse), typeof(TimeResponseExampleProvider))]
        public ActionResult<TimeResponse> Get()
        {
            var formatProvider = CultureInfo.InvariantCulture;
            var now = Clock.GetCurrentInstant().ToDateTimeOffset();

            return new TimeResponse()
            {
                Timestamp = now,
                Rfc1123 = now.ToString("r", formatProvider),
                UniversalFull = now.UtcDateTime.ToString("U", formatProvider),
                UniversalSortable = now.UtcDateTime.ToString("u", formatProvider),
                Unix = now.ToUnixTimeSeconds(),
            };
        }
    }
}
