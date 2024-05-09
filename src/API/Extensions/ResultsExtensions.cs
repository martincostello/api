// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Microsoft.AspNetCore.Http;

/// <summary>
/// A class containing extension methods for the <see cref="IResultExtensions"/> interface. This class cannot be inherited.
/// </summary>
internal static class ResultsExtensions
{
    /// <summary>
    /// Creates a new <see cref="IResult"/> that writes the specified HTML to the response.
    /// </summary>
    /// <param name="extensions">The <see cref="IResultExtensions"/> to extend.</param>
    /// <param name="html">The HTML to write to the write to the response.</param>
    /// <returns>
    /// The <see cref="IResult"/> that writes the specified HTML to the response.
    /// </returns>
    public static IResult Html(this IResultExtensions extensions, string html)
    {
        ArgumentNullException.ThrowIfNull(extensions);
        return new HtmlResult(html);
    }

    /// <summary>
    /// Returns an <see cref="IResult"/> representing an invalid API request.
    /// </summary>
    /// <param name="resultExtensions">The <see cref="IResultExtensions"/> being extended.</param>
    /// <param name="detail">The error detail.</param>
    /// <returns>
    /// The <see cref="ProblemHttpResult"/> representing the response.
    /// </returns>
    public static ProblemHttpResult InvalidRequest(this IResultExtensions resultExtensions, string detail)
    {
        ArgumentNullException.ThrowIfNull(resultExtensions);
        return TypedResults.Problem(detail, statusCode: StatusCodes.Status400BadRequest);
    }

    private sealed class HtmlResult(string html) : IResult
    {
        public async Task ExecuteAsync(HttpContext httpContext)
        {
            httpContext.Response.ContentType = "text/html; charset=utf-8";
            await httpContext.Response.WriteAsync(html, Encoding.UTF8, httpContext.RequestAborted);
        }
    }
}
