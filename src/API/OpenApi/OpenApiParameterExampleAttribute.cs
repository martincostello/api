// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api.OpenApi;

/// <summary>
/// An attribute representing an example for an OpenAPI operation parameter. This class cannot be inherited.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class OpenApiParameterExampleAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OpenApiParameterExampleAttribute"/> class.
    /// </summary>
    /// <param name="value">The example value.</param>
    public OpenApiParameterExampleAttribute(object value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the example value.
    /// </summary>
    public object Value { get; }
}
