// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api.Integration
{
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Shouldly;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// A class containing tests for loading resources in the website.
    /// </summary>
    public class ResourceTests : IntegrationTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceTests"/> class.
        /// </summary>
        /// <param name="fixture">The fixture to use.</param>
        /// <param name="outputHelper">The test output helper to use.</param>
        public ResourceTests(TestServerFixture fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
        }

        [Theory]
        [InlineData("/", "text/html")]
        [InlineData("/apple-touch-icon.png", "image/png")]
        [InlineData("/Assets/css/site.css", "text/css")]
        [InlineData("/Assets/css/site.min.css", "text/css")]
        [InlineData("/Assets/js/site.js", "application/javascript")]
        [InlineData("/Assets/js/site.min.js", "application/javascript")]
        [InlineData("BingSiteAuth.xml", "text/xml")]
        [InlineData("browserconfig.xml", "text/xml")]
        [InlineData("/docs", "text/html")]
        [InlineData("/error.html", "text/html")]
        [InlineData("/favicon.ico", "image/x-icon")]
        [InlineData("/googled1107923138d0b79.html", "text/html")]
        [InlineData("/gss.xsl", "text/xml")]
        [InlineData("/home/index", "text/html")]
        [InlineData("/home/index/", "text/html")]
        [InlineData("/HOME/INDEX", "text/html")]
        [InlineData("/humans.txt", "text/plain")]
        [InlineData("/keybase.txt", "text/plain")]
        [InlineData("/robots.txt", "text/plain")]
        [InlineData("/sitemap.xml", "text/xml")]
        [InlineData("/swagger/api/swagger.json", "application/json")]
        [InlineData("/time", "application/json")]
        [InlineData("/tools/guid", "application/json")]
        [InlineData("/tools/machinekey?decryptionAlgorithm=3DES&validationAlgorithm=3DES", "application/json")]
        public async Task Can_Load_Resource_As_Get(string requestUri, string contentType)
        {
            // Arrange
            using (var client = Fixture.CreateClient())
            {
                // Act
                using (var response = await client.GetAsync(requestUri))
                {
                    // Assert
                    response.StatusCode.ShouldBe(HttpStatusCode.OK);
                    response.Content.Headers.ContentType?.MediaType?.ShouldBe(contentType);
                }
            }
        }

        [Theory]
        [InlineData("/", "text/html")]
        public async Task Can_Load_Resource_As_Head(string requestUri, string contentType)
        {
            // Arrange
            using (var client = Fixture.CreateClient())
            {
                using (var message = new HttpRequestMessage(HttpMethod.Head, requestUri))
                {
                    // Act
                    using (var response = await client.SendAsync(message))
                    {
                        // Assert
                        response.StatusCode.ShouldBe(HttpStatusCode.OK);
                        response.Content.Headers.ContentType?.MediaType?.ShouldBe(contentType);
                    }
                }
            }
        }

        [Theory]
        [InlineData("/", "application/json")]
        public async Task Can_Load_Resource_As_Json(string requestUri, string contentType)
        {
            // Arrange
            using (var client = Fixture.CreateClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Act
                using (var response = await client.GetAsync(requestUri))
                {
                    // Assert
                    response.StatusCode.ShouldBe(HttpStatusCode.OK);
                    response.Content.Headers.ContentType?.MediaType?.ShouldBe(contentType);
                }
            }
        }

        [Fact]
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

            using (var client = Fixture.CreateClient())
            {
                // Act
                using (var response = await client.GetAsync("/"))
                {
                    // Assert
                    foreach (string expected in expectedHeaders)
                    {
                        response.Headers.Contains(expected).ShouldBeTrue($"The '{expected}' response header was not found.");
                    }
                }
            }
        }

        [Theory]
        [InlineData("/admin.php", HttpStatusCode.Found)]
        [InlineData("/blog", HttpStatusCode.Found)]
        [InlineData("/foo", HttpStatusCode.NotFound)]
        [InlineData("/error", HttpStatusCode.InternalServerError)]
        [InlineData("/error?id=399", HttpStatusCode.InternalServerError)]
        [InlineData("/error?id=400", HttpStatusCode.BadRequest)]
        [InlineData("/error?id=600", HttpStatusCode.InternalServerError)]
        public async Task Can_Load_Resource(string requestUri, HttpStatusCode expected)
        {
            // Arrange
            using (var client = Fixture.CreateClient(new WebApplicationFactoryClientOptions() { AllowAutoRedirect = false }))
            {
                // Act
                using (var response = await client.GetAsync(requestUri))
                {
                    // Assert
                    response.StatusCode.ShouldBe(expected, $"Incorrect status code for {requestUri}");
                }
            }
        }
    }
}
