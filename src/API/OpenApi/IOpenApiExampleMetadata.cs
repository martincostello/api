// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Text.Json;
using Microsoft.OpenApi.Any;

namespace MartinCostello.Api.OpenApi;

/// <summary>
/// Defines an OpenAPI example.
/// </summary>
public interface IOpenApiExampleMetadata
{
    /// <summary>
    /// Gets the type of the schema associated with the example.
    /// </summary>
    Type SchemaType { get; }

    /// <summary>
    /// Generates an example for the schema.
    /// </summary>
    /// <param name="options">The JSON serializer options to use.</param>
    /// <returns>
    /// The example to use.
    /// </returns>
    IOpenApiAny GenerateExample(JsonSerializerOptions options);
}
