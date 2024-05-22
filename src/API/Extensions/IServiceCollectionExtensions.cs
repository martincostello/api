// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using MartinCostello.Api.OpenApi.NSwag;
using MartinCostello.Api.Options;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

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

        services.AddOpenApi("api", (options) =>
        {
            options.UseTransformer<OpenApiDocumentTransformer>();
        });

        services.AddEndpointsApiExplorer();
        services.AddOpenApiDocument((options, services) =>
        {
            var siteOptions = services.GetRequiredService<IOptions<SiteOptions>>().Value;

            options.DocumentName = "api";
            options.Title = siteOptions.Metadata?.Name;
            options.Version = string.Empty;

            options.PostProcess = (document) =>
            {
                document.Generator = null;

                document.Info.Contact = new()
                {
                    Name = siteOptions.Metadata?.Author?.Name,
                    Url = siteOptions.Metadata?.Author?.Website ?? string.Empty,
                };

                document.Info.Description = siteOptions.Metadata?.Description;

                document.Info.License = new()
                {
                    Name = siteOptions.Api?.License?.Name,
                    Url = siteOptions.Api?.License?.Url ?? string.Empty,
                };

                document.Info.Version = string.Empty;
            };

            options.OperationProcessors.Add(new RemoveParameterPositionProcessor());
            options.OperationProcessors.Add(new UpdateProblemDetailsMediaTypeProcessor());
            options.SchemaSettings.SchemaProcessors.Add(new RemoveStyleCopPrefixesProcessor());
        });

        return services;
    }

    private sealed class OpenApiDocumentTransformer(IOptions<SiteOptions> options) : IOpenApiDocumentTransformer
    {
        public async Task TransformAsync(
            OpenApiDocument document,
            OpenApiDocumentTransformerContext context,
            CancellationToken cancellationToken)
        {
            var siteOptions = options.Value;

            document.Info.Title = siteOptions.Metadata?.Name;
            document.Info.Version = string.Empty;

            document.Info.Contact = new()
            {
                Name = siteOptions.Metadata?.Author?.Name,
            };

            if (siteOptions.Metadata?.Author?.Website is { } contactUrl)
            {
                document.Info.Contact.Url = new(contactUrl);
            }

            document.Info.Description = siteOptions.Metadata?.Description;

            document.Info.License = new()
            {
                Name = siteOptions.Api?.License?.Name,
            };

            if (siteOptions.Api?.License?.Url is { } licenseUrl)
            {
                document.Info.License.Url = new(licenseUrl);
            }

            document.Info.Version = string.Empty;

            await Task.CompletedTask;
        }
    }
}
