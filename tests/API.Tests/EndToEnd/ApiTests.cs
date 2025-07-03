// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using Microsoft.AspNetCore.WebUtilities;

namespace MartinCostello.Api.EndToEnd;

public partial class ApiTests(ApiFixture fixture) : EndToEndTest(fixture)
{
    private static readonly TimeSpan Tolerance = TimeSpan.FromSeconds(5);

    [Fact]
    public async Task Can_Get_Time_With_Json()
    {
        // Arrange
        var utcNow = DateTimeOffset.UtcNow;
        using var client = Fixture.CreateClient();

        // Act
        using var response = await client.GetFromJsonAsync("/time", AppJsonSerializerContext.Default.JsonDocument, CancellationToken);

        // Assert
        response.ShouldNotBeNull();
        response.RootElement.GetProperty("timestamp").GetDateTimeOffset().ShouldBe(utcNow, Tolerance);

        DateTimeOffset.TryParse(response.RootElement.GetProperty("rfc1123").GetString(), out var actual).ShouldBeTrue();
        actual.ShouldBe(utcNow, Tolerance, "rfc1123 is not a valid DateTimeOffset.");

        DateTimeOffset.TryParse(response.RootElement.GetProperty("universalSortable").GetString(), out actual).ShouldBeTrue();
        actual.ShouldBe(utcNow, Tolerance, "universalSortable is not a valid DateTimeOffset.");

        DateTimeOffset.TryParse(response.RootElement.GetProperty("universalFull").GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out actual).ShouldBeTrue();
        actual.ShouldBe(utcNow, Tolerance, "universalFull is not a valid DateTimeOffset.");

        long unix = response.RootElement.GetProperty("unix").GetInt64();
        unix.ShouldBeGreaterThan(DateTimeOffset.UnixEpoch.ToUnixTimeSeconds());
        DateTimeOffset.FromUnixTimeSeconds(unix).ShouldBe(utcNow, Tolerance, "The value of unix is incorrect.");
    }

    [Fact]
    public async Task Can_Get_Time_With_Grpc()
    {
        // Arrange
        var utcNow = DateTimeOffset.UtcNow;

        using var channel = Fixture.CreateGrpcChannel();

        var client = new Time.TimeClient(channel);

        // Act
        var actual = await client.NowAsync(new(), cancellationToken: CancellationToken);

        // Assert
        actual.ShouldNotBeNull();

        DateTimeOffset.TryParse(actual.Timestamp, out var value).ShouldBeTrue();
        value.ShouldBe(utcNow, Tolerance, "timestamp is not a valid DateTimeOffset.");

        DateTimeOffset.TryParse(actual.Rfc1123, out value).ShouldBeTrue();
        value.ShouldBe(utcNow, Tolerance, "rfc1123 is not a valid DateTimeOffset.");

        DateTimeOffset.TryParse(actual.UniversalSortable, out value).ShouldBeTrue();
        value.ShouldBe(utcNow, Tolerance, "universalSortable is not a valid DateTimeOffset.");

        DateTimeOffset.TryParse(actual.UniversalFull, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out value).ShouldBeTrue();
        value.ShouldBe(utcNow, Tolerance, "universalFull is not a valid DateTimeOffset.");

        actual.Unix.ShouldBeGreaterThan(DateTimeOffset.UnixEpoch.ToUnixTimeSeconds());
        DateTimeOffset.FromUnixTimeSeconds(actual.Unix).ShouldBe(utcNow, Tolerance, "The value of unix is incorrect.");
    }

    [Fact]
    public async Task Can_Generate_Guid()
    {
        // Arrange
        using var client = Fixture.CreateClient();

        // Act
        using var response = await client.GetFromJsonAsync("/tools/guid", AppJsonSerializerContext.Default.JsonDocument, CancellationToken);

        // Assert
        response.ShouldNotBeNull();
        response.RootElement.GetProperty("guid").GetGuid().ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Can_Generate_Machine_Key()
    {
        // Arrange
        var parameters = new Dictionary<string, string?>()
        {
            ["decryptionAlgorithm"] = "AES-256",
            ["validationAlgorithm"] = "SHA1",
        };

        using var client = Fixture.CreateClient();

        string requestUri = QueryHelpers.AddQueryString("/tools/machinekey", parameters);

        // Act
        using var response = await client.GetFromJsonAsync(requestUri, AppJsonSerializerContext.Default.JsonDocument, CancellationToken);

        // Assert
        response.ShouldNotBeNull();
        response.RootElement.GetProperty("decryptionKey").GetString().ShouldNotBeNullOrWhiteSpace();
        response.RootElement.GetProperty("validationKey").GetString().ShouldNotBeNullOrWhiteSpace();
        response.RootElement.GetProperty("machineKeyXml").GetString().ShouldNotBeNullOrWhiteSpace();

        var element = XElement.Parse(response.RootElement.GetProperty("machineKeyXml").GetString()!);

        element.Name.ShouldBe("machineKey");
        element.Attribute("decryption")!.Value.ShouldBe("AES");
        element.Attribute("decryptionKey")!.Value.ShouldBe(response.RootElement.GetProperty("decryptionKey").GetString());
        element.Attribute("validation")!.Value.ShouldBe("SHA1");
        element.Attribute("validationKey")!.Value.ShouldBe(response.RootElement.GetProperty("validationKey").GetString());
    }

    [Theory]
    [InlineData("md5", "hexadecimal", "martincostello.com", "e6c3105bdb8e6466f9db1dab47a85131")]
    [InlineData("sha1", "hexadecimal", "martincostello.com", "7fbd8e8cf806e5282af895396f5268483bf6af1b")]
    [InlineData("sha256", "hexadecimal", "martincostello.com", "3b8143aa8119eaf0910aef5cade45dd0e6bb7b70e8d1c8c057bf3fc125248642")]
    [InlineData("sha384", "hexadecimal", "martincostello.com", "5c0e892a9348c184df255f46ab7282eb5792d552c896eb6893d90f36c7202540a9942c80ce5812616d29c08331c60510")]
    [InlineData("sha512", "hexadecimal", "martincostello.com", "3be0167275455dcf1e34f8818d48b7ae4a61fb8549153f42d0d035464fdccee97022d663549eb249d4796956e4016ad83d5e64ba766fb751c8fb2c03b2b4eb9a")]
    public async Task Can_Generate_Hash(string algorithm, string format, string plaintext, string expected)
    {
        // Arrange
        var request = new JsonObject()
        {
            ["algorithm"] = algorithm,
            ["format"] = format,
            ["plaintext"] = plaintext,
        };

        using var client = Fixture.CreateClient();

        // Act
        using var response = await client.PostAsJsonAsync("/tools/hash", request, AppJsonSerializerContext.Default.JsonObject, CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();

        using var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(CancellationToken), cancellationToken: CancellationToken);

        result.RootElement.GetProperty("hash").GetString().ShouldBe(expected);
    }

    [JsonSerializable(typeof(JsonDocument))]
    [JsonSerializable(typeof(JsonObject))]
    private sealed partial class AppJsonSerializerContext : JsonSerializerContext;
}
