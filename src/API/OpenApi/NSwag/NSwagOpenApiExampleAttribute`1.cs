// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using NSwag.Annotations;

namespace MartinCostello.Api.OpenApi.NSwag;

/// <summary>
/// Defines an attribute for an OpenAPI schema example.
/// </summary>
/// <typeparam name="T">The type of the schema.</typeparam>
public sealed class NSwagOpenApiExampleAttribute<T>() : OpenApiOperationProcessorAttribute(typeof(OpenApiExampleProcessor<T, T>))
    where T : IExampleProvider<T>;
