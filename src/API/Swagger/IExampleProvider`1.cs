// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api.Swagger;

/// <summary>
/// Defines a method for obtaining examples for Swagger documentation.
/// </summary>
/// <typeparam name="T">The type of the example.</typeparam>
internal interface IExampleProvider<T> : IExampleProvider
{
}
