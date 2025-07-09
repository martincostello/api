// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Resources;
using Pyroscope;

namespace MartinCostello.Api;

/// <summary>
/// A class containing telemetry information for the API.
/// </summary>
public static class ApplicationTelemetry
{
    /// <summary>
    /// The name of the service.
    /// </summary>
    public static readonly string ServiceName = "API";

    /// <summary>
    /// The version of the service.
    /// </summary>
    public static readonly string ServiceVersion = GitMetadata.Version.Split('+')[0];

    /// <summary>
    /// The custom activity source for the service.
    /// </summary>
    public static readonly ActivitySource ActivitySource = new(ServiceName, ServiceVersion);

    /// <summary>
    /// Gets the <see cref="ResourceBuilder"/> to use for telemetry.
    /// </summary>
    public static ResourceBuilder ResourceBuilder { get; } = ResourceBuilder.CreateDefault()
        .AddService(ServiceName, ServiceName, ServiceVersion)
        .AddAzureAppServiceDetector()
        .AddContainerDetector()
        .AddHostDetector()
        .AddOperatingSystemDetector()
        .AddProcessRuntimeDetector();

    /// <summary>
    /// Returns whether an OTLP collector is configured.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if OTLP is configured; otherwise <see langword="false"/>.
    /// </returns>
    internal static bool IsOtlpCollectorConfigured()
        => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT"));

    /// <summary>
    /// Returns whether Pyroscope is configured.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if Pyroscope is configured; otherwise <see langword="false"/>.
    /// </returns>
    internal static bool IsPyroscopeConfigured()
        => Environment.GetEnvironmentVariable("PYROSCOPE_PROFILING_ENABLED") is "1";

    /// <summary>
    /// Profiles the specified delegate with the labels from the current span's baggage, if any.
    /// </summary>
    /// <typeparam name="T">The type of the state.</typeparam>
    /// <param name="state">The state to pass to the operation.</param>
    /// <param name="operation">The operation to profile.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation.
    /// </returns>
    [StackTraceHidden]
    internal static async Task ProfileAsync<T>(T state, Func<T, Task> operation)
    {
        if (ExtractK6Baggage() is not { Count: > 0 } baggage)
        {
            await operation(state);
            return;
        }

        try
        {
            Profiler.Instance.ClearDynamicTags();

            foreach ((string key, string value) in baggage)
            {
                Profiler.Instance.SetDynamicTag(key, value);
            }

            await operation(state);
        }
        finally
        {
            Profiler.Instance.ClearDynamicTags();
        }
    }

    private static Dictionary<string, string>? ExtractK6Baggage()
    {
        // Based on https://github.com/grafana/pyroscope-go/blob/8fff2bccb5ed5611fdb09fdbd9a727367ab35f39/x/k6/baggage.go
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

                // See https://grafana.com/docs/k6/latest/javascript-api/jslib/http-instrumentation-pyroscope/#about-baggage-header
                labels ??= new(3);
                labels[label] = value;
            }
        }

        return labels;
    }
}
