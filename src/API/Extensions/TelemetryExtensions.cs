// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.Tracing;
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
    /// <param name="builder">The <see cref="IHostApplicationBuilder"/> to configure telemetry for.</param>
    public static void AddTelemetry(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddMetrics();
        builder.Services.AddTracing((p) => p.AddConfiguration(builder.Configuration.GetSection("Tracing")));

        var telemetry = builder.Services.AddOpenTelemetry();

        if (ApplicationTelemetry.IsOtlpCollectorConfigured())
        {
            telemetry.UseOtlpExporter();
        }

        telemetry
            .WithMetrics((metrics) =>
            {
                metrics.SetResourceBuilder(ApplicationTelemetry.ResourceBuilder)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddProcessInstrumentation()
                        .SetExemplarFilter(ExemplarFilterType.TraceBased);
            })
            .WithTracing((tracing) =>
            {
                tracing.SetResourceBuilder(ApplicationTelemetry.ResourceBuilder)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation();

                if (builder.Environment.IsDevelopment())
                {
                    tracing.SetSampler(new AlwaysOnSampler());
                }

                if (ApplicationTelemetry.IsPyroscopeConfigured())
                {
                    tracing.AddProcessor(new Pyroscope.OpenTelemetry.PyroscopeSpanProcessor());
                }
            });

        builder.Services
            .AddOptions<HttpClientTraceInstrumentationOptions>()
            .Configure((options) =>
            {
                options.EnrichWithHttpResponseMessage = EnrichHttpActivity;
                options.RecordException = true;
            });

        builder.Logging.AddTelemetry();
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
