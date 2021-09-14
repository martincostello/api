// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Mvc;

namespace MartinCostello.Api.Swagger;

/// <summary>
/// A class representing an implementation of <see cref="IExampleProvider"/>
/// for the <see cref="ProblemDetails"/> class. This class cannot be inherited.
/// </summary>
public sealed class ProblemDetailsExampleProvider : IExampleProvider<ProblemDetails>
{
    /// <inheritdoc />
    public object GetExample()
    {
        return new ProblemDetails()
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Bad Request",
            Status = StatusCodes.Status400BadRequest,
            Detail = "The specified value is invalid.",
        };
    }
}
