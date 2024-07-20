// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MartinCostello.Api.Integration;

/// <summary>
/// A class containing tests for loading resources in the website.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ResourceTests"/> class.
/// </remarks>
/// <param name="fixture">The fixture to use.</param>
/// <param name="outputHelper">The test output helper to use.</param>
[Collection(TestServerCollection.Name)]
public class ResourceTests(TestServerFixture fixture, ITestOutputHelper outputHelper) : IntegrationTest(fixture, outputHelper)
{
    [Theory]
    [InlineData("/", MediaTypeNames.Text.Html)]
    [InlineData("/apple-touch-icon.png", "image/png")]
    [InlineData("/assets/css/main.css", "text/css")]
    [InlineData("/assets/css/main.css.map", MediaTypeNames.Text.Plain)]
    [InlineData("/assets/js/main.js", "text/javascript")]
    [InlineData("/assets/js/main.js.map", MediaTypeNames.Text.Plain)]
    [InlineData("BingSiteAuth.xml", MediaTypeNames.Text.Xml)]
    [InlineData("browserconfig.xml", MediaTypeNames.Text.Xml)]
    [InlineData("/docs", MediaTypeNames.Text.Html)]
    [InlineData("/error.html", MediaTypeNames.Text.Html)]
    [InlineData("/favicon.ico", "image/x-icon")]
    [InlineData("/googled1107923138d0b79.html", MediaTypeNames.Text.Html)]
    [InlineData("/gss.xsl", MediaTypeNames.Text.Xml)]
    [InlineData("/humans.txt", MediaTypeNames.Text.Plain)]
    [InlineData("/keybase.txt", MediaTypeNames.Text.Plain)]
    [InlineData("/openapi/api.json", MediaTypeNames.Application.Json)]
    [InlineData("/robots.txt", MediaTypeNames.Text.Plain)]
    [InlineData("/robots933456.txt", MediaTypeNames.Text.Plain)]
    [InlineData("/sitemap.xml", MediaTypeNames.Text.Xml)]
    [InlineData("/time", MediaTypeNames.Application.Json)]
    [InlineData("/tools/guid", MediaTypeNames.Application.Json)]
    [InlineData("/tools/guid?uppercase=false", MediaTypeNames.Application.Json)]
    [InlineData("/tools/guid?uppercase=true", MediaTypeNames.Application.Json)]
    [InlineData("/tools/machinekey?decryptionAlgorithm=3DES&validationAlgorithm=3DES", MediaTypeNames.Application.Json)]
    [InlineData("/version", MediaTypeNames.Application.Json)]
    public async Task Can_Load_Resource_As_Get(string requestUri, string contentType)
    {
        // Arrange
        using var client = Fixture.CreateClient();

        // Act
        using var response = await client.GetAsync(requestUri);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.ShouldNotBeNull();
        response.Content!.Headers.ContentType?.MediaType?.ShouldBe(contentType);
    }

    [Theory]
    [InlineData("/", MediaTypeNames.Text.Html)]
    public async Task Can_Load_Resource_As_Head(string requestUri, string contentType)
    {
        // Arrange
        using var client = Fixture.CreateClient();
        using var message = new HttpRequestMessage(HttpMethod.Head, requestUri);

        // Act
        using var response = await client.SendAsync(message);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.ShouldNotBeNull();
        response.Content!.Headers.ContentType?.MediaType?.ShouldBe(contentType);
    }

    [Fact]
    public async Task Response_Headers_Contains_Expected_Headers()
    {
        // Arrange
        string[] expectedHeaders =
        [
            "content-security-policy",
            "Permissions-Policy",
            "Referrer-Policy",
            "X-Content-Type-Options",
            "X-Download-Options",
            "X-Frame-Options",
            "X-Instance",
            "X-Request-Id",
            "X-Revision",
            "X-XSS-Protection",
        ];

        using var client = Fixture.CreateClient();

        // Act
        using var response = await client.GetAsync("/");

        // Assert
        foreach (string expected in expectedHeaders)
        {
            response.Headers.Contains(expected).ShouldBeTrue($"The '{expected}' response header was not found.");
        }
    }

    [Theory]
    [InlineData("/foo", HttpStatusCode.NotFound)]
    [InlineData("/error", HttpStatusCode.InternalServerError)]
    [InlineData("/error?id=399", HttpStatusCode.InternalServerError)]
    [InlineData("/error?id=400", HttpStatusCode.BadRequest)]
    [InlineData("/error?id=600", HttpStatusCode.InternalServerError)]
    public async Task Can_Load_Resource(string requestUri, HttpStatusCode expected)
    {
        // Arrange
        using var client = Fixture.CreateClient(new WebApplicationFactoryClientOptions() { AllowAutoRedirect = false });

        // Act
        using var response = await client.GetAsync(requestUri);

        // Assert
        response.StatusCode.ShouldBe(expected, $"Incorrect status code for {requestUri}");
    }
}
