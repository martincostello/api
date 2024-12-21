// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Validations;

namespace MartinCostello.Api.Integration;

[Collection<TestServerCollection>]
public class OpenApiTests(TestServerFixture fixture, ITestOutputHelper outputHelper) : IntegrationTest(fixture, outputHelper)
{
    [Fact]
    public async Task Json_Schema_Is_Correct()
    {
        // Arrange
        var settings = new VerifySettings();
        settings.DontScrubGuids();

        using var client = Fixture.CreateClient();

        // Act
        string actual = await client.GetStringAsync("/openapi/api.json", CancellationToken);

        // Assert
        await VerifyJson(actual, settings);
    }

    [Fact]
    public async Task Yaml_Schema_Is_Correct()
    {
        // Arrange
        var settings = new VerifySettings();
        settings.DontScrubGuids();

        using var client = Fixture.CreateClient();

        // Act
        string actual = await client.GetStringAsync("/openapi/api.yaml", CancellationToken);

        // Assert
        await Verify(actual, settings);
    }

    [Theory]
    [InlineData("/openapi/api.json")]
    [InlineData("/openapi/api.yaml")]
    public async Task Schema_Has_No_Validation_Warnings(string requestUrl)
    {
        // Arrange
        var ruleSet = ValidationRuleSet.GetDefaultRuleSet();

        // HACK Workaround for https://github.com/microsoft/OpenAPI.NET/issues/1738
        ruleSet.Remove("MediaTypeMismatchedDataType");

        using var client = Fixture.CreateClient();

        // Act
        using var schema = await client.GetStreamAsync(requestUrl, CancellationToken);

        // Assert
        var reader = new OpenApiStreamReader();
        var actual = await reader.ReadAsync(schema, CancellationToken);

        actual.OpenApiDiagnostic.Errors.ShouldBeEmpty();

        var errors = actual.OpenApiDocument.Validate(ruleSet);
        errors.ShouldBeEmpty();
    }
}
