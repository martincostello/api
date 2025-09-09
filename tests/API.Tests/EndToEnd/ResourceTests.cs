// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Net;
using System.Net.Mime;

namespace MartinCostello.Api.EndToEnd;

public class ResourceTests(ApiFixture fixture) : EndToEndTest(fixture)
{
    [Theory]
    [InlineData("/", MediaTypeNames.Text.Html)]
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
    [InlineData("/openapi/api.yaml", MediaTypeNames.Application.Yaml)]
    [InlineData("/robots.txt", MediaTypeNames.Text.Plain)]
    [InlineData("/robots933456.txt", MediaTypeNames.Text.Plain)]
    [InlineData("/sitemap.xml", MediaTypeNames.Text.Xml)]
    [InlineData("/time", MediaTypeNames.Application.Json)]
    [InlineData("/tools/guid", MediaTypeNames.Application.Json)]
    [InlineData("/tools/machinekey?decryptionAlgorithm=3DES&validationAlgorithm=3DES", MediaTypeNames.Application.Json)]
    [InlineData("/version", MediaTypeNames.Application.Json)]
    public async Task Can_Load_Resource_As_Get(string requestUri, string contentType)
    {
        // Arrange
        using var client = Fixture.CreateClient();

        // Act
        using var response = await client.GetAsync(requestUri, CancellationToken);

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
            "Cross-Origin-Embedder-Policy",
            "Cross-Origin-Opener-Policy",
            "Cross-Origin-Resource-Policy",
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
        using var response = await client.GetAsync("/", CancellationToken);

        // Assert
        foreach (string expected in expectedHeaders)
        {
            response.Headers.Contains(expected).ShouldBeTrue($"The '{expected}' response header was not found.");
        }
    }
}
