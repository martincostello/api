// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace MartinCostello.Api.OpenApi;

/// <summary>
/// A class representing a document processor that removes StyleCop
/// prefixes from property descriptions. This class cannot be inherited.
/// </summary>
internal sealed class RemoveStyleCopPrefixesTransformer : IOpenApiDocumentTransformer
{
    private const string Prefix = "Gets or sets ";

    /// <inheritdoc/>
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (document.Components?.Schemas is { } schemas)
        {
            foreach (var schema in schemas)
            {
                foreach (var property in schema.Value.Properties.Values)
                {
                    TryUpdateDescription(property);
                }
            }
        }

        foreach (var path in document.Paths.Values)
        {
            foreach (var operation in path.Operations.Values)
            {
                foreach (var response in operation.Responses.Values)
                {
                    foreach (var model in response.Content.Values)
                    {
                        foreach (var property in model.Schema.Properties.Values)
                        {
                            TryUpdateDescription(property);
                        }
                    }
                }
            }
        }

        return Task.CompletedTask;
    }

    private static void TryUpdateDescription(OpenApiSchema property)
    {
        if (property.Description is not null)
        {
            property.Description = property.Description.Replace(Prefix, string.Empty, StringComparison.Ordinal);
            property.Description = char.ToUpperInvariant(property.Description[0]) + property.Description[1..];
        }
    }
}
