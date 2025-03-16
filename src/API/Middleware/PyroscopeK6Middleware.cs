// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

//// Based on https://github.com/grafana/pyroscope-go/blob/8fff2bccb5ed5611fdb09fdbd9a727367ab35f39/x/k6/baggage.go

using OpenTelemetry;
using Pyroscope;

namespace MartinCostello.Api.Middleware;

/// <summary>
/// A class representing middleware for adding profiler labels for Pyroscope from k6. This class cannot be inherited.
/// </summary>
internal sealed class PyroscopeK6Middleware(RequestDelegate next, ILogger<PyroscopeK6Middleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<PyroscopeK6Middleware> _logger = logger;

    /// <summary>
    /// Invokes the middleware asynchronously.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the actions performed by the middleware.
    /// </returns>
    public async Task InvokeAsync(HttpContext context)
    {
        if (ExtractK6Baggage() is { Count: > 0 } baggage)
        {
            try
            {
                Profiler.Instance.ClearDynamicTags();

                foreach ((string key, string value) in baggage)
                {
                    Profiler.Instance.SetDynamicTag(key, value);
                }

                await _next(context).ConfigureAwait(false);
            }
            finally
            {
                Profiler.Instance.ClearDynamicTags();
            }
        }
        else
        {
            await _next(context).ConfigureAwait(false);
        }
    }

    private Dictionary<string, string>? ExtractK6Baggage()
    {
#pragma warning disable CA1848 // Use the LoggerMessage delegates
        _logger.LogInformation("Baggage count: {Count}", Baggage.GetBaggage().Count);
#pragma warning restore CA1848 // Use the LoggerMessage delegates

        if (Baggage.GetBaggage() is not { Count: > 0 } baggage)
        {
            return null;
        }

        Dictionary<string, string>? labels = null;

        foreach ((string key, string? value) in baggage.Where((p) => p.Key.StartsWith("k6.", StringComparison.Ordinal)))
        {
            if (value is { Length: > 0 })
            {
                string label = key.Replace('.', '_');

                // See https://github.com/grafana/jslib.k6.io/blob/80255ea7b239d3d4a8cd4e82192eef4ba27941a2/lib/http-instrumentation-pyroscope/1.0.1/index.js#L11
                labels ??= new(3);
                labels[label] = value;
            }
        }

        return labels;
    }
}
