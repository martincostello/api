// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using MartinCostello.Api.OpenApi;
using MartinCostello.Api.Options;
using MartinCostello.OpenApi;
using Microsoft.Extensions.Options;

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
        services.AddHttpContextAccessor();
        services.AddSingleton<IPostConfigureOptions<OpenApiExtensionsOptions>, PostConfigureOpenApiExtensionsOptions>();

        const string DocumentName = "api";

        services.AddOpenApi(DocumentName, (options) =>
        {
            options.AddDocumentTransformer<AddApiInfo>();

            var original = options.CreateSchemaReferenceId;
            options.CreateSchemaReferenceId = (type) =>
            {
                if (type.Type == typeof(HttpValidationProblemDetails))
                {
                    return "ProblemDetails";
                }

                return original(type);
            };
        });

        services.AddOpenApiExtensions(DocumentName, (options) =>
        {
            options.AddExamples = true;
            options.AddServerUrls = true;
            options.SerializationContexts.Add(ApplicationJsonSerializerContext.Default);

            options.AddExample<HttpValidationProblemDetails, HttpValidationProblemDetailsExampleProvider>();
        });

        return services;
    }

    private sealed class PostConfigureOpenApiExtensionsOptions(IOptionsMonitor<SiteOptions> monitor) : IPostConfigureOptions<OpenApiExtensionsOptions>
    {
        public void PostConfigure(string? name, OpenApiExtensionsOptions options)
            => options.DefaultServerUrl = $"https://{monitor.CurrentValue.Metadata!.Domain}";
    }
}
