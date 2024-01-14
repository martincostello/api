// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using NJsonSchema.Generation;

namespace MartinCostello.Api.OpenApi;

/// <summary>
/// A class representing a document processor that removes StyleCop
/// prefixes from property descriptions. This class cannot be inherited.
/// </summary>
public sealed class RemoveStyleCopPrefixesProcessor : ISchemaProcessor
{
    private const string Prefix = "Gets or sets ";

    /// <inheritdoc/>
    public void Process(SchemaProcessorContext context)
    {
        foreach ((_, var property) in context.Schema.ActualProperties)
        {
            if (property.Description is not null)
            {
                property.Description = property.Description.Replace(Prefix, string.Empty, StringComparison.Ordinal);
                property.Description = char.ToUpperInvariant(property.Description[0]) + property.Description[1..];
            }
        }
    }
}
