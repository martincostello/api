// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Net;
using System.Text.Json;
using JustEat.HttpClientInterception;

namespace MartinCostello.Api.Integration;

/// <summary>
/// A class containing tests for the <c>/github</c> endpoints.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TimeTests"/> class.
/// </remarks>
/// <param name="fixture">The fixture to use.</param>
/// <param name="outputHelper">The test output helper to use.</param>
[Collection<TestServerCollection>]
public class GitHubTests(TestServerFixture fixture, ITestOutputHelper outputHelper) : IntegrationTest(fixture, outputHelper)
{
    [Fact]
    public async Task GitHub_Can_Get_Device_Code()
    {
        // Arrange
        var builder = new HttpRequestInterceptionBuilder()
            .ForPost()
            .ForUrl("https://github.com/login/device/code?client_id=dkd73mfo9ASgjsfnhJD8&scope=public_repo")
            .WithJsonContent(
                new()
                {
                    DeviceCode = "3584d83530557fdd1f46af8289938c8ef79f9dc5",
                    ExpiresInSeconds = 900,
                    RefreshIntervalInSeconds = 5,
                    UserCode = "WDJB-MJHT",
                    VerificationUrl = "https://github.com/login/device",
                },
                SerializerContext.GitHubDeviceCode);

        builder.RegisterWith(Fixture.Interceptor);

        using var client = Fixture.CreateClient();

        // Act
        var response = await client.PostAsync(
            "/github/login/device/code?client_id=dkd73mfo9ASgjsfnhJD8&scope=public_repo",
            null,
            CancellationToken);

        string content = await response.Content.ReadAsStringAsync(CancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.OK, content);

        var actual = JsonSerializer.Deserialize(content, SerializerContext.GitHubDeviceCode);

        // Assert
        actual.ShouldNotBeNull();
        actual.DeviceCode.ShouldBe("3584d83530557fdd1f46af8289938c8ef79f9dc5");
        actual.ExpiresInSeconds.ShouldBe(900);
        actual.RefreshIntervalInSeconds.ShouldBe(5);
        actual.UserCode.ShouldBe("WDJB-MJHT");
        actual.VerificationUrl.ShouldBe("https://github.com/login/device");
    }

    [Fact]
    public async Task GitHub_Can_Get_Access_Code()
    {
        // Arrange
        var builder = new HttpRequestInterceptionBuilder()
            .ForPost()
            .ForUrl("https://github.com/login/oauth/access_token?client_id=dkd73mfo9ASgjsfnhJD8&device_code=3584d83530557fdd1f46af8289938c8ef79f9dc5&grant_type=urn%3Aietf%3Aparams%3Aoauth%3Agrant-type%3Adevice_code")
            .WithJsonContent(
                new()
                {
                    AccessToken = "not_a_real_token",
                    TokenType = "bearer",
                    Scopes = "public_repo",
                },
                SerializerContext.GitHubAccessToken);

        builder.RegisterWith(Fixture.Interceptor);

        using var client = Fixture.CreateClient();

        // Act
        var response = await client.PostAsync(
            "/github/login/oauth/access_token?client_id=dkd73mfo9ASgjsfnhJD8&device_code=3584d83530557fdd1f46af8289938c8ef79f9dc5&grant_type=urn:ietf:params:oauth:grant-type:device_code",
            null,
            CancellationToken);

        string content = await response.Content.ReadAsStringAsync(CancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.OK, content);

        var actual = JsonSerializer.Deserialize(content, SerializerContext.GitHubAccessToken);

        // Assert
        actual.ShouldNotBeNull();
        actual.AccessToken.ShouldBe("not_a_real_token");
        actual.Error.ShouldBeNull();
        actual.TokenType.ShouldBe("bearer");
        actual.Scopes.ShouldBe("public_repo");
    }
}
