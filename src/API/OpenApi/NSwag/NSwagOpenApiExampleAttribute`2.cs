// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using NSwag.Annotations;

namespace MartinCostello.Api.OpenApi.NSwag;

/// <summary>
/// Defines an attribute for an OpenAPI schema example.
/// </summary>
/// <typeparam name="TSchema">The type of the schema.</typeparam>
/// <typeparam name="TProvider">The type of the example provider.</typeparam>
public sealed class NSwagOpenApiExampleAttribute<TSchema, TProvider>() : OpenApiOperationProcessorAttribute(typeof(OpenApiExampleProcessor<TSchema, TProvider>))
    where TProvider : IExampleProvider<TSchema>;
