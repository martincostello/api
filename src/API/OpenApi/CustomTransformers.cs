// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace MartinCostello.Api.OpenApi;

/// <summary>
/// A class containing methods for transforming OpenAPI operations and schemas. This class cannot be inherited.
/// </summary>
internal static class CustomTransformers
{
    private static readonly IOpenApiOperationTransformer[] OperationTransformers =
    [
        new AddExamplesTransformer(),
        new AddResponseDescriptionTransformer(),
        new RemoveStyleCopPrefixesTransformer(),
        new AddOperationIdTransformer(), // HACK See https://github.com/dotnet/aspnetcore/issues/55838
    ];

    private static readonly IOpenApiSchemaTransformer[] SchemaTransformers =
    [
        new AddSchemaDescriptionsTransformer(),
        new AddExamplesTransformer(),
        new RemoveStyleCopPrefixesTransformer(),
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
        foreach (var transformer in OperationTransformers)
        {
            await transformer.TransformAsync(operation, context, cancellationToken);
        }
    }

    /// <summary>
    /// Runs any schema transformers associated with the schema.
    /// </summary>
    /// <param name="schema">The <see cref="OpenApiSchema"/> to transform.</param>
    /// <param name="context">The <see cref="OpenApiOperationTransformerContext"/> associated with the <see paramref="schema"/>.</param>
    /// <param name="cancellationToken">The cancellation token to use.</param>
    /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
    internal static async Task TransformSchemas(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        foreach (var transformer in SchemaTransformers)
        {
            await transformer.TransformAsync(schema, context, cancellationToken);
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
