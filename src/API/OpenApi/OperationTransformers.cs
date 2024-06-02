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
    /// <summary>
    /// Adds response examples to the specified OpenAPI operation.
    /// </summary>
    /// <param name="operation">The <see cref="OpenApiOperation"/> to modify.</param>
    /// <param name="context">The <see cref="OpenApiOperationTransformerContext"/> associated with the <see paramref="operation"/>.</param>
    /// <param name="cancellationToken">The cancellation token to use.</param>
    /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
    internal static async Task AddResponseExamples(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var transformers = context.Description.ActionDescriptor.EndpointMetadata
            .OfType<IOpenApiOperationTransformer>()
            .ToArray();

        foreach (var transformer in transformers)
        {
            await transformer.TransformAsync(operation, context, cancellationToken);
        }
    }
}
