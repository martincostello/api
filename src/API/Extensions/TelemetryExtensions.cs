// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Instrumentation.Http;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace MartinCostello.Api.Extensions;

/// <summary>
/// A class containing telemetry-related extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class TelemetryExtensions
{
    /// <summary>
    /// Adds telemetry services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure telemetry for.</param>
    /// <param name="environment">The current <see cref="IWebHostEnvironment"/>.</param>
    public static void AddTelemetry(this IServiceCollection services, IWebHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(services);

        var builder = services.AddOpenTelemetry();

        if (ApplicationTelemetry.IsOtlpCollectorConfigured())
        {
            builder.UseOtlpExporter();
        }

        builder.WithMetrics((builder) =>
               {
                   builder.SetResourceBuilder(ApplicationTelemetry.ResourceBuilder)
                          .AddAspNetCoreInstrumentation()
                          .AddHttpClientInstrumentation()
                          .AddProcessInstrumentation()
                          .AddMeter("System.Runtime")
                          .SetExemplarFilter(ExemplarFilterType.TraceBased);
               })
               .WithTracing((builder) =>
               {
                   builder.SetResourceBuilder(ApplicationTelemetry.ResourceBuilder)
                          .AddAspNetCoreInstrumentation()
                          .AddHttpClientInstrumentation()
                          .AddSource(ApplicationTelemetry.ServiceName);

                   if (environment.IsDevelopment())
                   {
                       builder.SetSampler(new AlwaysOnSampler());
                   }

                   if (ApplicationTelemetry.IsPyroscopeConfigured())
                   {
                       builder.AddProcessor(new Pyroscope.OpenTelemetry.PyroscopeSpanProcessor());
                   }
               });

        services.AddOptions<HttpClientTraceInstrumentationOptions>()
                .Configure((options) =>
                {
                    options.EnrichWithHttpRequestMessage = EnrichHttpActivity;
                    options.EnrichWithHttpResponseMessage = EnrichHttpActivity;
                    options.RecordException = true;
                });
    }

    private static void EnrichHttpActivity(Activity activity, HttpRequestMessage request)
    {
        if (GetTag("server.address", activity.Tags) is { Length: > 0 } hostName)
        {
            activity.AddTag("peer.service", hostName);
        }

        static string? GetTag(string name, IEnumerable<KeyValuePair<string, string?>> tags)
            => tags.FirstOrDefault((p) => p.Key == name).Value;
    }

    private static void EnrichHttpActivity(Activity activity, HttpResponseMessage response)
    {
        if (response.RequestMessage?.Headers.TryGetValues("x-ms-client-request-id", out var clientRequestId) is true)
        {
            activity.SetTag("az.client_request_id", clientRequestId);
        }

        if (response.Headers.TryGetValues("x-ms-request-id", out var requestId))
        {
            activity.SetTag("az.service_request_id", requestId);
        }
    }
}
