// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api.Swagger;

/// <summary>
/// Defines an example request for an API method. This class cannot be inherited.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SwaggerRequestExampleAttribute"/> class.
/// </remarks>
/// <param name="requestType">The type of the request.</param>
/// <param name="exampleType">The type of the example.</param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
internal sealed class SwaggerRequestExampleAttribute(Type requestType, Type exampleType) : Attribute
{
    /// <summary>
    /// Gets the type of the request.
    /// </summary>
    public Type RequestType { get; } = requestType;

    /// <summary>
    /// Gets the type of the example.
    /// </summary>
    public Type ExampleType { get; } = exampleType;
}
