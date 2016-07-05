// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api.Controllers
{
    using Models;
    using MyTested.AspNetCore.Mvc;
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
            var initial = NodaTime.Instant.FromUtc(2016, 05, 24, 12, 34, 56);
            var clock = new NodaTime.Testing.FakeClock(initial);

            MyMvc.Controller(() => new TimeController(clock))
                 .Calling((p) => p.Get())
                 .ShouldReturn()
                 .Ok()
                 .WithStatusCode(HttpStatusCode.OK)
                 .AndAlso()
                 .WithResponseModelOfType<TimeResponse>()
                 .Passing((p) =>
                     {
                         p.Timestamp.ShouldBe(initial.ToDateTimeOffset());
                         p.Rfc1123.ShouldBe("Tue, 24 May 2016 12:34:56 GMT");
                         p.UniversalFull.ShouldBe("Tuesday, 24 May 2016 12:34:56");
                         p.UniversalSortable.ShouldBe("2016-05-24 12:34:56Z");
                         p.Unix.ShouldBe(1464093296);
                     });
        }
    }
}
