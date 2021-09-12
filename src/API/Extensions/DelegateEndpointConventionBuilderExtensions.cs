// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using MartinCostello.Api.Swagger;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MartinCostello.Api.Extensions;

/// <summary>
/// A class containing extension methods for the <see cref="DelegateEndpointConventionBuilder"/> class. This class cannot be inherited.
/// </summary>
internal static class DelegateEndpointConventionBuilderExtensions
{
    /// <summary>
    /// Adds <see cref="SwaggerResponseAttribute"/> to the metadata for all builders produced by builder.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="builder">The <see cref="DelegateEndpointConventionBuilder"/>.</param>
    /// <param name="description">The response description.</param>
    /// <param name="statusCode">The response status code. Defaults to <see cref="StatusCodes.Status200OK"/>.</param>
    /// <param name="contentType">The response content type. Defaults to <c>application/json</c>.</param>
    /// <returns>
    /// A <see cref="DelegateEndpointConventionBuilder"/> that can be used to further customize the endpoint.
    /// </returns>
    internal static DelegateEndpointConventionBuilder Produces<TResponse>(
        this DelegateEndpointConventionBuilder builder,
        string description,
        int statusCode = StatusCodes.Status200OK,
        string? contentType = null)
    {
        return builder
            .Produces<TResponse>(statusCode, contentType)
            .WithMetadata(new SwaggerResponseAttribute(statusCode) { Type = typeof(TResponse), Description = description });
    }

    /// <summary>
    /// Adds <see cref="SwaggerResponseAttribute"/> to the metadata for all builders produced by builder.
    /// </summary>
    /// <param name="builder">The <see cref="DelegateEndpointConventionBuilder"/>.</param>
    /// <param name="description">The response description.</param>
    /// <param name="statusCode">The response status code. Defaults to <see cref="StatusCodes.Status400BadRequest"/>.</param>
    /// <returns>
    /// A <see cref="DelegateEndpointConventionBuilder"/> that can be used to further customize the endpoint.
    /// </returns>
    internal static DelegateEndpointConventionBuilder ProducesProblem(
        this DelegateEndpointConventionBuilder builder,
        string description,
        int statusCode = StatusCodes.Status400BadRequest)
    {
        return builder.Produces<ProblemDetails>(
            description,
            statusCode,
            "application/problem+json");
    }

    /// <summary>
    /// Adds the <see cref="SwaggerOperationAttribute"/> to the metadata for all builders produced by builder.
    /// </summary>
    /// <param name="builder">The <see cref="DelegateEndpointConventionBuilder"/>.</param>
    /// <param name="summary">The operation summary.</param>
    /// <param name="description">The optional operation description.</param>
    /// <returns>
    /// A <see cref="DelegateEndpointConventionBuilder"/> that can be used to further customize the endpoint.
    /// </returns>
    internal static DelegateEndpointConventionBuilder WithOperationDescription(
        this DelegateEndpointConventionBuilder builder,
        string summary,
        string? description = null)
    {
        return builder.WithMetadata(new SwaggerOperationAttribute(summary, description));
    }

    /// <summary>
    /// Adds the <see cref="SwaggerResponseExampleAttribute"/> for <see cref="ProblemDetails"/> to the metadata for all builders produced by builder.
    /// </summary>
    /// <param name="builder">The <see cref="DelegateEndpointConventionBuilder"/>.</param>
    /// <returns>
    /// A <see cref="DelegateEndpointConventionBuilder"/> that can be used to further customize the endpoint.
    /// </returns>
    internal static DelegateEndpointConventionBuilder WithProblemDetailsResponseExample(this DelegateEndpointConventionBuilder builder)
    {
        return builder.WithResponseExample<ProblemDetails, ProblemDetailsExampleProvider>();
    }

    /// <summary>
    /// Adds the <see cref="SwaggerRequestExampleAttribute"/> to the metadata for all builders produced by builder.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TExampleProvider">The type of the example provider.</typeparam>
    /// <param name="builder">The <see cref="DelegateEndpointConventionBuilder"/>.</param>
    /// <returns>
    /// A <see cref="DelegateEndpointConventionBuilder"/> that can be used to further customize the endpoint.
    /// </returns>
    internal static DelegateEndpointConventionBuilder WithRequestExample<TRequest, TExampleProvider>(this DelegateEndpointConventionBuilder builder)
        where TExampleProvider : IExampleProvider
    {
        return builder.WithMetadata(new SwaggerRequestExampleAttribute(typeof(TRequest), typeof(TExampleProvider)));
    }

    /// <summary>
    /// Adds the <see cref="SwaggerResponseExampleAttribute"/> to the metadata for all builders produced by builder.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <typeparam name="TExampleProvider">The type of the example provider.</typeparam>
    /// <param name="builder">The <see cref="DelegateEndpointConventionBuilder"/>.</param>
    /// <returns>
    /// A <see cref="DelegateEndpointConventionBuilder"/> that can be used to further customize the endpoint.
    /// </returns>
    internal static DelegateEndpointConventionBuilder WithResponseExample<TResponse, TExampleProvider>(this DelegateEndpointConventionBuilder builder)
        where TExampleProvider : IExampleProvider
    {
        return builder.WithMetadata(new SwaggerResponseExampleAttribute(typeof(TResponse), typeof(TExampleProvider)));
    }
}
