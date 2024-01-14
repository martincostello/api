// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using MartinCostello.Api.OpenApi;
using MartinCostello.Api.Options;
using Microsoft.Extensions.Options;

namespace MartinCostello.Api.Extensions;

/// <summary>
/// A class containing extension methods for the <see cref="IServiceCollection"/> interface. This class cannot be inherited.
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds OpenAPI to the services.
    /// </summary>
    /// <param name="value">The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <returns>
    /// The value specified by <paramref name="value"/>.
    /// </returns>
    public static IServiceCollection AddOpenApi(this IServiceCollection value)
    {
        value.AddOpenApiDocument((options, services) =>
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
            options.OperationProcessors.Add(new UpdateProblemDetailsMediaTypeProvider());
            options.SchemaSettings.SchemaProcessors.Add(new RemoveStyleCopPrefixesProcessor());
        });

        return value;
    }
}
