// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using NodaTime.Testing;
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
            var clock = new FakeClock(initial);

            IActionResult result;

            using (var target = new TimeController(clock))
            {
                // Act
                result = target.Get();
            }

            // Assert
            result.ShouldNotBeNull();

            var actual = result
                .ShouldBeOfType<JsonResult>()
                .Value.ShouldBeOfType<TimeResponse>();

            actual.Timestamp.ShouldBe(initial.ToDateTimeOffset());
            actual.Rfc1123.ShouldBe("Tue, 24 May 2016 12:34:56 GMT");
            actual.UniversalFull.ShouldBe("Tuesday, 24 May 2016 12:34:56");
            actual.UniversalSortable.ShouldBe("2016-05-24 12:34:56Z");
            actual.Unix.ShouldBe(1464093296);
        }
    }
}
