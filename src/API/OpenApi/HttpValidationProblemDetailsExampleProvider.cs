// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using MartinCostello.OpenApi;

namespace MartinCostello.Api.OpenApi;

/// <summary>
/// A class representing an example provider for <see cref="HttpValidationProblemDetails"/>.
/// </summary>
internal sealed class HttpValidationProblemDetailsExampleProvider : IExampleProvider<HttpValidationProblemDetails>
{
    /// <inheritdoc/>
    public static HttpValidationProblemDetails GenerateExample()
    {
        return new()
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Bad Request",
            Status = StatusCodes.Status400BadRequest,
            Detail = "The specified value is invalid.",
        };
    }
}
