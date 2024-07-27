// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MartinCostello.Api.OpenApi;

/// <summary>
/// A class representing that modifies XML documentation for operations and schemas that matches <c>StyleCop</c>
/// requirements to be more human-readable for display in OpenAPI documentation. This class cannot be inherited.
/// </summary>
internal sealed class RemoveStyleCopPrefixes : IOperationFilter, ISchemaFilter
{
    private const string Prefix = "Gets or sets ";

    /// <inheritdoc/>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
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

    /// <inheritdoc/>
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Description is not null)
        {
            schema.Description = schema.Description.Replace("`", string.Empty, StringComparison.Ordinal);
        }

        foreach (var property in schema.Properties.Values)
        {
            TryUpdateDescription(property);
        }
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
