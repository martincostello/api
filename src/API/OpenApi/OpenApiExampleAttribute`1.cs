// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api.OpenApi;

/// <summary>
/// An attribute representing an example for an OpenAPI operation parameter.
/// </summary>
/// <typeparam name="T">The type of the schema.</typeparam>
internal sealed class OpenApiExampleAttribute<T>() : OpenApiExampleAttribute<T, T>()
    where T : IExampleProvider<T>;
