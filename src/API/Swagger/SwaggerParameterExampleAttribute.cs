// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using Swashbuckle.AspNetCore.Annotations;

namespace MartinCostello.Api.Swagger;

/// <summary>
/// Defines a Swagger operation parameter with an example value. This class cannot be inherited.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SwaggerParameterExampleAttribute"/> class.
/// </remarks>
/// <param name="description">The description of the parameter.</param>
/// <param name="example">The optional example value.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class SwaggerParameterExampleAttribute(string description, object? example = null) : SwaggerParameterAttribute(description)
{
    /// <summary>
    /// Gets the example value for the parameter.
    /// </summary>
    public object? Example { get; } = example;
}
