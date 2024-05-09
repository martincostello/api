﻿// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using FluentAssertions.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace MartinCostello.Api.Integration;

[Collection(TestServerCollection.Name)]
public class OpenApiTests(TestServerFixture fixture, ITestOutputHelper outputHelper) : IntegrationTest(fixture, outputHelper)
{
    [Fact]
    public async Task Static_And_Dynamic_Schema_Should_Match()
    {
        // Arrange
        var requestUri = new Uri("/swagger/api/swagger.json", UriKind.Relative);
        string subpath = "swagger/api/swagger.json";

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
}