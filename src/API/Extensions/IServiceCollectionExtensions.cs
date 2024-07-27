// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using MartinCostello.Api.OpenApi;
using MartinCostello.Api.Options;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MartinCostello.Api.Extensions;

/// <summary>
/// A class containing extension methods for the <see cref="IServiceCollection"/> interface. This class cannot be inherited.
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds OpenAPI documentation to the services.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add OpenAPI documentation to.</param>
    /// <returns>
    /// The value specified by <paramref name="services"/>.
    /// </returns>
    public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services)
    {
        if (!RuntimeFeature.IsDynamicCodeSupported)
        {
            return services;
        }

        services.AddEndpointsApiExplorer();
        services.AddHttpContextAccessor();

        services.AddSwaggerGen((options) =>
        {
            using var provider = services.BuildServiceProvider();
            var siteOptions = provider.GetRequiredService<IOptions<SiteOptions>>().Value;

            var info = new OpenApiInfo()
            {
                Contact = new()
                {
                    Name = siteOptions.Metadata?.Author?.Name,
                    Url = new(siteOptions.Metadata?.Author?.Website ?? string.Empty),
                },
                Description = siteOptions.Metadata?.Description,
                License = new()
                {
                    Name = siteOptions.Api?.License?.Name,
                    Url = new(siteOptions.Api?.License?.Url ?? string.Empty),
                },
                Title = siteOptions.Metadata?.Name,
                Version = string.Empty,
            };

            options.SwaggerDoc("api", info);

            options.EnableAnnotations();
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "API.xml"));

            options.DocumentFilter<AddDocumentTags>();
            options.DocumentFilter<AddServers>();
            options.OperationFilter<AddDescriptions>();

            var examples = new AddExamples();
            options.AddOperationFilterInstance(examples);
            options.AddSchemaFilterInstance(examples);

            var prefixes = new RemoveStyleCopPrefixes();
            options.AddOperationFilterInstance(prefixes);
            options.AddSchemaFilterInstance(prefixes);
        });

        return services;
    }

    private sealed class AddDocumentTags : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
            => swaggerDoc.Tags.Add(new() { Name = "API" });
    }
}
