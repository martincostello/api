// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using MartinCostello.Api.OpenApi;
using MartinCostello.OpenApi;

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
        // HACK Enable when https://github.com/dotnet/aspnetcore/issues/56023 is fixed
        if (!RuntimeFeature.IsDynamicCodeSupported)
        {
            return services;
        }

        services.AddHttpContextAccessor();

        const string DocumentName = "api";

        services.AddOpenApi(DocumentName, (options) =>
        {
            options.UseTransformer<AddApiInfo>();
        });

        services.AddOpenApiExtensions(DocumentName, (options) =>
        {
            options.AddExamples = true;
            options.AddServerUrls = true;
            options.SerializationContext = ApplicationJsonSerializerContext.Default;

            options.AddXmlComments<Program>();
        });

        return services;
    }
}
