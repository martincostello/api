// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Http.HttpResults;

#pragma warning disable IDE0130
namespace Microsoft.AspNetCore.Http;

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
    /// The <see cref="ValidationProblem"/> representing the response.
    /// </returns>
    public static ValidationProblem InvalidRequest(this IResultExtensions resultExtensions, string detail)
    {
        ArgumentNullException.ThrowIfNull(resultExtensions);
        return TypedResults.ValidationProblem([], detail);
    }
}
