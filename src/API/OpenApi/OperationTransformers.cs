// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace MartinCostello.Api.OpenApi;

/// <summary>
/// A class containing methods for transforming OpenAPI operations. This class cannot be inherited.
/// </summary>
internal static class OperationTransformers
{
    private static readonly IOpenApiOperationTransformer[] Transformers =
    [
        new AddExamplesOperationTransformer(),
        new AddResponseDescriptionTransformer(),
        new AddOperationIdTransformer(), // HACK See https://github.com/dotnet/aspnetcore/issues/55838
    ];

    /// <summary>
    /// Runs any operation transformers associated with the operation.
    /// </summary>
    /// <param name="operation">The <see cref="OpenApiOperation"/> to transform.</param>
    /// <param name="context">The <see cref="OpenApiOperationTransformerContext"/> associated with the <see paramref="operation"/>.</param>
    /// <param name="cancellationToken">The cancellation token to use.</param>
    /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
    internal static async Task TransformOperations(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        foreach (var transformer in Transformers)
        {
            await transformer.TransformAsync(operation, context, cancellationToken);
        }
    }

    private sealed class AddOperationIdTransformer : IOpenApiOperationTransformer
    {
        public Task TransformAsync(
            OpenApiOperation operation,
            OpenApiOperationTransformerContext context,
            CancellationToken cancellationToken)
        {
            operation.OperationId =
                context.Description.ActionDescriptor.AttributeRouteInfo?.Name ??
                context.Description.ActionDescriptor.EndpointMetadata.OfType<IEndpointNameMetadata>().LastOrDefault()?.EndpointName;

            return Task.CompletedTask;
        }
    }
}
