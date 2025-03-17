// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api.Middleware;

/// <summary>
/// A class representing middleware for adding profiler labels for Pyroscope from k6. This class cannot be inherited.
/// </summary>
internal sealed class PyroscopeK6Middleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    /// <summary>
    /// Invokes the middleware asynchronously.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the actions performed by the middleware.
    /// </returns>
    [System.Diagnostics.StackTraceHidden]
    public Task InvokeAsync(HttpContext context) =>
        ApplicationTelemetry.ProfileAsync((_next, context), static (state) => state._next(state.context));
}
