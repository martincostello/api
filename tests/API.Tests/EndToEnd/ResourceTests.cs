// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace MartinCostello.Api.EndToEnd
{
    public class ResourceTests : EndToEndTest
    {
        public ResourceTests(ApiFixture fixture)
            : base(fixture)
        {
        }

        [SkippableTheory]
        [InlineData("/", MediaTypeNames.Text.Html)]
        [InlineData("/apple-touch-icon.png", "image/png")]
        [InlineData("/Assets/css/site.css", "text/css")]
        [InlineData("/Assets/css/site.min.css", "text/css")]
        [InlineData("/Assets/js/site.js", "application/javascript")]
        [InlineData("/Assets/js/site.min.js", "application/javascript")]
        [InlineData("BingSiteAuth.xml", MediaTypeNames.Text.Xml)]
        [InlineData("browserconfig.xml", MediaTypeNames.Text.Xml)]
        [InlineData("/docs", MediaTypeNames.Text.Html)]
        [InlineData("/error.html", MediaTypeNames.Text.Html)]
        [InlineData("/favicon.ico", "image/x-icon")]
        [InlineData("/googled1107923138d0b79.html", MediaTypeNames.Text.Html)]
        [InlineData("/gss.xsl", MediaTypeNames.Text.Xml)]
        [InlineData("/home/index", MediaTypeNames.Text.Html)]
        [InlineData("/home/index/", MediaTypeNames.Text.Html)]
        [InlineData("/HOME/INDEX", MediaTypeNames.Text.Html)]
        [InlineData("/humans.txt", MediaTypeNames.Text.Plain)]
        [InlineData("/keybase.txt", MediaTypeNames.Text.Plain)]
        [InlineData("/robots.txt", MediaTypeNames.Text.Plain)]
        [InlineData("/sitemap.xml", MediaTypeNames.Text.Xml)]
        [InlineData("/swagger/api/swagger.json", MediaTypeNames.Application.Json)]
        [InlineData("/time", MediaTypeNames.Application.Json)]
        [InlineData("/tools/guid", MediaTypeNames.Application.Json)]
        [InlineData("/tools/machinekey?decryptionAlgorithm=3DES&validationAlgorithm=3DES", MediaTypeNames.Application.Json)]
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

        [SkippableFact]
        public async Task Response_Headers_Contains_Expected_Headers()
        {
            // Arrange
            string[] expectedHeaders = new[]
            {
                "content-security-policy",
                "feature-policy",
                "Referrer-Policy",
                "X-Content-Type-Options",
                "X-Datacenter",
                "X-Download-Options",
                "X-Frame-Options",
                "X-Instance",
                "X-Request-Id",
                "X-Revision",
                "X-XSS-Protection",
            };

            using var client = Fixture.CreateClient();

            // Act
            using var response = await client.GetAsync("/");

            // Assert
            foreach (string expected in expectedHeaders)
            {
                response.Headers.Contains(expected).ShouldBeTrue($"The '{expected}' response header was not found.");
            }
        }
    }
}
