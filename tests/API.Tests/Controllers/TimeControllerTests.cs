// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimeControllerTests.cs" company="https://martincostello.com/">
//   Martin Costello (c) 2016
// </copyright>
// <summary>
//   TimeControllerTests.cs
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MartinCostello.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using Xunit;

    /// <summary>
    /// A class containing tests for the <see cref="TimeController"/> class. This class cannot be inherited.
    /// </summary>
    public static class TimeControllerTests
    {
        [Fact]
        public static void Time_Get_Returns_Correct_Response()
        {
            // Arrange
            var initial = NodaTime.Instant.FromUtc(2016, 05, 24, 12, 34, 56);
            var clock = new NodaTime.Testing.FakeClock(initial);

            IActionResult result;

            using (var target = new TimeController(clock))
            {
                // Act
                result = target.Get();
            }

            // Assert
            Assert.NotNull(result);
            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.NotNull(objectResult.Value);
            var model = Assert.IsType<TimeResponse>(objectResult.Value);

            Assert.Equal("Tue, 24 May 2016 12:34:56 GMT", model.Rfc1123);
            Assert.Equal("Tuesday, 24 May 2016 12:34:56", model.UniversalFull);
            Assert.Equal("2016-05-24 12:34:56Z", model.UniversalSortable);
            Assert.Equal(1464093296, model.Unix);
        }
    }
}
