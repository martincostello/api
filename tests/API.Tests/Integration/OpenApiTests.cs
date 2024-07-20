// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using FluentAssertions.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Validations;
using Newtonsoft.Json.Linq;

namespace MartinCostello.Api.Integration;

[Collection(TestServerCollection.Name)]
public class OpenApiTests(TestServerFixture fixture, ITestOutputHelper outputHelper) : IntegrationTest(fixture, outputHelper)
{
    [Theory]
    [InlineData("/swagger/api/openapi.json")]
    [InlineData("/swagger/api/swagger.json")]
    public async Task Static_And_Dynamic_Schema_Match(string subpath)
    {
        // Arrange
        var requestUri = new Uri(subpath, UriKind.Relative);

        var environment = Fixture.Services.GetRequiredService<IWebHostEnvironment>();

        var schema = environment.WebRootFileProvider.GetFileInfo(subpath);
        schema.Exists.ShouldBeTrue();

        using var stream = schema.CreateReadStream();
        using var reader = new StreamReader(stream);
        string expectedJson = await reader.ReadToEndAsync();

        using var client = Fixture.CreateClient();

        // Act
        string actualJson = await client.GetStringAsync(requestUri);

        // Use System.Text.Json equivalent when available.
        // See https://github.com/fluentassertions/fluentassertions/issues/2205.
        var expected = JToken.Parse(expectedJson);
        var actual = JToken.Parse(actualJson);

        // Servers is removed because the URL won't neccessarily match.
        expected["servers"]?.Parent?.Remove();
        actual["servers"]?.Parent?.Remove();

        // Assert
        string customMessage =
            $"""
            The static OpenAPI schema differs from the dynamically generated one.
            If the changes are intentional, please update the static schema file at
            '{subpath}' with the latest content returned by an HTTP GET
            to the {requestUri} endpoint.
            """;

        actual.Should().BeEquivalentTo(expected, customMessage.Trim());
    }

    [Theory]
    [InlineData("/swagger/api/openapi.json")]
    [InlineData("/swagger/api/swagger.json")]
    public async Task Schema_Has_No_Validation_Warnings(string requestUrl)
    {
        // Arrange
        var ruleSet = ValidationRuleSet.GetDefaultRuleSet();

        // HACK Workaround for https://github.com/microsoft/OpenAPI.NET/issues/1738
        ruleSet.Remove("MediaTypeMismatchedDataType");

        using var client = Fixture.CreateClient();

        // Act
        using var schema = await client.GetStreamAsync(requestUrl);

        // Assert
        var reader = new OpenApiStreamReader();
        var actual = await reader.ReadAsync(schema);

        actual.OpenApiDiagnostic.Errors.ShouldBeEmpty();

        var errors = actual.OpenApiDocument.Validate(ruleSet);
        errors.ShouldBeEmpty();
    }
}
