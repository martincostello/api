﻿// --------------------------------------------------------------------------------------------------------------------
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
    using Shouldly;
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
            result.ShouldNotBeNull();

            var objectResult = result.ShouldBeOfType<ObjectResult>();

            objectResult.Value.ShouldNotBeNull();

            var model = objectResult.Value.ShouldBeOfType<TimeResponse>();

            model.Timestamp.ShouldBe(initial.ToDateTimeOffset());
            model.Rfc1123.ShouldBe("Tue, 24 May 2016 12:34:56 GMT");
            model.UniversalFull.ShouldBe("Tuesday, 24 May 2016 12:34:56");
            model.UniversalSortable.ShouldBe("2016-05-24 12:34:56Z");
            model.Unix.ShouldBe(1464093296);
        }
    }
}
