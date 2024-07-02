// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api.OpenApi;

/// <summary>
/// Defines a method for obtaining examples for OpenAPI documentation.
/// </summary>
/// <typeparam name="T">The type of the example.</typeparam>
public interface IExampleProvider<T>
{
    /// <summary>
    /// Generates the example to use.
    /// </summary>
    /// <returns>
    /// A <typeparamref name="T"/> that should be used as the example.
    /// </returns>
    static abstract T GenerateExample();
}
