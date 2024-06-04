// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using MartinCostello.Api.OpenApi;
using Microsoft.AspNetCore.OpenApi;
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
            options.UseTransformer<AddApiInfoTransformer>();
            options.UseTransformer<RemoveStyleCopPrefixesTransformer>();

            options.UseOperationTransformer(OperationTransformers.TransformOperations);

            // HACK See https://github.com/dotnet/aspnetcore/issues/55832
            options.UseTransformer<ScrubExtensionsTransformer>();
        });

        return services;
    }

    private sealed class ScrubExtensionsTransformer : IOpenApiDocumentTransformer
    {
        public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
        {
            foreach (var pathItem in document.Paths.Values)
            {
                foreach (var operation in pathItem.Operations.Values)
                {
                    operation.Extensions.Remove("x-aspnetcore-id");
                }
            }

            return Task.CompletedTask;
        }
    }
}
