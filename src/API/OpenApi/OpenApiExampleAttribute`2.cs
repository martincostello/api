﻿// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;
using Microsoft.OpenApi.Any;

namespace MartinCostello.Api.OpenApi;

#pragma warning disable CA1813

/// <summary>
/// An attribute representing an example for an OpenAPI operation parameter.
/// </summary>
/// <typeparam name="TSchema">The type of the schema.</typeparam>
/// <typeparam name="TProvider">The type of the example provider.</typeparam>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = true)]
internal class OpenApiExampleAttribute<TSchema, TProvider> : Attribute, IOpenApiExampleMetadata
    where TProvider : IExampleProvider<TSchema>
{
    /// <inheritdoc/>
    public Type SchemaType { get; } = typeof(TSchema);

    /// <summary>
    /// Generates the example to use.
    /// </summary>
    /// <returns>
    /// A <typeparamref name="TSchema"/> that should be used as the example.
    /// </returns>
    public virtual TSchema GenerateExample() => TProvider.GenerateExample();

    /// <inheritdoc/>
    IOpenApiAny IOpenApiExampleMetadata.GenerateExample(JsonSerializerContext context)
        => ExampleFormatter.AsJson(GenerateExample(), context);
}
