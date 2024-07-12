// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Net.Http.Json;

namespace MartinCostello.Api.Integration;

/// <summary>
/// A class containing tests for the <c>/time</c> endpoint.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TimeTests"/> class.
/// </remarks>
/// <param name="fixture">The fixture to use.</param>
/// <param name="outputHelper">The test output helper to use.</param>
[Collection(TestServerCollection.Name)]
public class TimeTests(TestServerFixture fixture, ITestOutputHelper outputHelper) : IntegrationTest(fixture, outputHelper)
{
    [Fact]
    public async Task Time_Get_Returns_Correct_Response()
    {
        // Arrange
        using var client = Fixture.CreateClient();

        // Act
        var actual = await client.GetFromJsonAsync("/time", ApplicationJsonSerializerContext.Default.TimeResponse);

        // Assert
        actual.ShouldNotBeNull();
        actual!.Timestamp.ShouldBe(new DateTimeOffset(2016, 05, 24, 12, 34, 56, TimeSpan.Zero));
        actual.Rfc1123.ShouldBe("Tue, 24 May 2016 12:34:56 GMT");
        actual.UniversalFull.ShouldBe("Tuesday, 24 May 2016 12:34:56");
        actual.UniversalSortable.ShouldBe("2016-05-24 12:34:56Z");
        actual.Unix.ShouldBe(1464093296);
    }
}
