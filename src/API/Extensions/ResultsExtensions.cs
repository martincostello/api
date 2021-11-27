// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Result;

namespace MartinCostello.Api.Extensions;

/// <summary>
/// A class containing extension methods for the <see cref="IResultExtensions"/> interface. This class cannot be inherited.
/// </summary>
internal static class ResultsExtensions
{
    /// <summary>
    /// Returns an <see cref="IResult"/> representing an invalid API request.
    /// </summary>
    /// <param name="resultExtensions">The <see cref="IResultExtensions"/> being extended.</param>
    /// <param name="detail">The error detail.</param>
    /// <returns>
    /// The <see cref="IResult"/> representing the response.
    /// </returns>
    public static IResult InvalidRequest(this IResultExtensions resultExtensions, string detail)
    {
        ArgumentNullException.ThrowIfNull(resultExtensions);
        return Results.Problem(detail, statusCode: StatusCodes.Status400BadRequest);
    }

    /// <summary>
    /// Creates a <see cref="IResult"/> that serializes the specified <paramref name="value"/> object to JSON.
    /// </summary>
    /// <typeparam name="T">The type of the value to write as JSON.</typeparam>
    /// <param name="extensions">The <see cref="IResultExtensions"/> being extended.</param>
    /// <param name="value">The object to write as JSON.</param>
    /// <param name="context">The serializer context to use when serializing the value.</param>
    /// <param name="contentType">The content-type to set on the response.</param>
    /// <param name="statusCode">The status code to set on the response.</param>
    /// <returns>
    /// The created <see cref="JsonResult"/> that serializes the specified
    /// <paramref name="value"/> as JSON format for the response.</returns>
    /// <remarks>
    /// Callers should cache an instance of serializer settings to avoid recreating cached data with each call.
    /// </remarks>
    public static IResult Json<T>(
        this IResultExtensions extensions,
        T? value,
        JsonSerializerContext? context = null,
        string? contentType = null,
        int? statusCode = null)
    {
        ArgumentNullException.ThrowIfNull(extensions);

        return new JsonResult
        {
            ContentType = contentType,
            InputType = typeof(T),
            JsonSerializerContext = context,
            StatusCode = statusCode,
            Value = value,
        };
    }
}
