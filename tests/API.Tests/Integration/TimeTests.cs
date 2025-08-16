// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Net.Http.Json;
using Grpc.Net.Client;

namespace MartinCostello.Api.Integration;

/// <summary>
/// A class containing tests for the <c>/time</c> endpoint.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TimeTests"/> class.
/// </remarks>
/// <param name="fixture">The fixture to use.</param>
/// <param name="outputHelper">The test output helper to use.</param>
[Collection<TestServerCollection>]
public class TimeTests(TestServerFixture fixture, ITestOutputHelper outputHelper) : IntegrationTest(fixture, outputHelper)
{
    [Fact]
    public async Task Get_Time_With_Json_Get_Returns_Correct_Response()
    {
        // Arrange
        using var client = Fixture.CreateClient();

        // Act
        var actual = await client.GetFromJsonAsync("/time", SerializerContext.TimeResponse, CancellationToken);

        // Assert
        actual.ShouldNotBeNull();
        actual.Timestamp.ShouldBe(new DateTimeOffset(2016, 05, 24, 12, 34, 56, TimeSpan.Zero));
        actual.Rfc1123.ShouldBe("Tue, 24 May 2016 12:34:56 GMT");
        actual.UniversalFull.ShouldBe("Tuesday, 24 May 2016 12:34:56");
        actual.UniversalSortable.ShouldBe("2016-05-24 12:34:56Z");
        actual.Unix.ShouldBe(1464093296);
    }

    [Fact]
    public async Task Get_Time_With_Grpc_Returns_Correct_Response()
    {
        // Arrange
        using var channel = GrpcChannel.ForAddress(Fixture.ClientOptions.BaseAddress, new()
        {
            HttpHandler = Fixture.Server.CreateHandler(),
            LoggerFactory = Fixture.OutputHelper!.ToLoggerFactory(),
        });

        var client = new Time.TimeClient(channel);

        // Act
        var actual = await client.NowAsync(new(), cancellationToken: CancellationToken);

        // Assert
        actual.ShouldNotBeNull();
        actual.Timestamp.ShouldBe("2016-05-24T12:34:56.0000000+00:00");
        actual.Rfc1123.ShouldBe("Tue, 24 May 2016 12:34:56 GMT");
        actual.UniversalFull.ShouldBe("Tuesday, 24 May 2016 12:34:56");
        actual.UniversalSortable.ShouldBe("2016-05-24 12:34:56Z");
        actual.Unix.ShouldBe(1464093296);
    }
}
