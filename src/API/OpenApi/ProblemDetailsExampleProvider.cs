// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using MartinCostello.OpenApi;
using Microsoft.AspNetCore.Mvc;

namespace MartinCostello.Api.OpenApi;

/// <summary>
/// A class representing an example provider for <see cref="ProblemDetails"/>.
/// </summary>
internal sealed class ProblemDetailsExampleProvider : IExampleProvider<ProblemDetails>
{
    /// <inheritdoc/>
    public static ProblemDetails GenerateExample()
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
