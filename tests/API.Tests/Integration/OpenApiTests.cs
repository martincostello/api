// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Reader;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Validations;

namespace MartinCostello.Api.Integration;

[Collection<TestServerCollection>]
public class OpenApiTests(TestServerFixture fixture, ITestOutputHelper outputHelper) : IntegrationTest(fixture, outputHelper)
{
    static OpenApiTests()
    {
        OpenApiReaderRegistry.RegisterReader(OpenApiConstants.Yaml, new OpenApiYamlReader());
    }

    [Fact(Skip = "https://github.com/dotnet/aspnetcore/issues/60630")]
    public async Task Json_Schema_Is_Correct()
    {
        // Arrange
        var settings = new VerifySettings();
        settings.DontScrubDateTimes();
        settings.DontScrubGuids();

        using var client = Fixture.CreateClient();

        // Act
        string actual = await client.GetStringAsync("/openapi/api.json", CancellationToken);

        // Assert
        await VerifyJson(actual, settings);
    }

    [Fact(Skip = "https://github.com/dotnet/aspnetcore/issues/60630")]
    public async Task Yaml_Schema_Is_Correct()
    {
        // Arrange
        var settings = new VerifySettings();
        settings.DontScrubDateTimes();
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
        string format = Path.GetExtension(requestUrl).TrimStart('.');
        var ruleSet = ValidationRuleSet.GetDefaultRuleSet();

        using var client = Fixture.CreateClient();

        // Act
        using var schema = await client.GetStreamAsync(requestUrl, CancellationToken);

        // Assert
        var actual = await OpenApiDocument.LoadAsync(schema, format, cancellationToken: CancellationToken);

        actual.Diagnostic.Errors.ShouldBeEmpty();

        var errors = actual.Document.Validate(ruleSet);
        errors.ShouldBeEmpty();
    }
}
