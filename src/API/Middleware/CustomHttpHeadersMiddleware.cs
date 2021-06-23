// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using MartinCostello.Api.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace MartinCostello.Api.Middleware
{
    /// <summary>
    /// A class representing middleware for adding custom HTTP response headers. This class cannot be inherited.
    /// </summary>
    public sealed class CustomHttpHeadersMiddleware
    {
        /// <summary>
        /// The delegate for the next part of the pipeline. This field is read-only.
        /// </summary>
        private readonly RequestDelegate _next;

        /// <summary>
        /// The current <c>Content-Security-Policy</c> HTTP response header value. This field is read-only.
        /// </summary>
        private readonly string _contentSecurityPolicy;

        /// <summary>
        /// The name of the current hosting environment. This field is read-only.
        /// </summary>
        private readonly string _environmentName;

        /// <summary>
        /// The current datacenter name. This field is read-only.
        /// </summary>
        private readonly string _datacenter;

        /// <summary>
        /// Whether the current hosting environment is production. This field is read-only.
        /// </summary>
        private readonly bool _isProduction;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomHttpHeadersMiddleware"/> class.
        /// </summary>
        /// <param name="next">The delegate for the next part of the pipeline.</param>
        /// <param name="environment">The current hosting environment.</param>
        /// <param name="config">The current configuration.</param>
        /// <param name="options">The current site configuration options.</param>
        public CustomHttpHeadersMiddleware(
            RequestDelegate next,
            IWebHostEnvironment environment,
            IConfiguration config,
            IOptions<SiteOptions> options)
        {
            _next = next;
            _isProduction = environment.IsProduction();
            _environmentName = (_isProduction ? null : environment.EnvironmentName) ?? string.Empty;
            _datacenter = config["Azure:Datacenter"] ?? "Local";
            _contentSecurityPolicy = BuildContentSecurityPolicy(_isProduction, options.Value);
        }

        /// <summary>
        /// Invokes the middleware asynchronously.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the actions performed by the middleware.
        /// </returns>
        public async Task Invoke(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            context.Response.OnStarting(() =>
                {
                    context.Response.Headers.Remove("Server");
                    context.Response.Headers.Remove("X-Powered-By");

                    context.Response.Headers.Add("Content-Security-Policy", _contentSecurityPolicy);
                    context.Response.Headers.Add("Feature-Policy", "accelerometer 'none'; camera 'none'; geolocation 'none'; gyroscope 'none'; magnetometer 'none'; microphone 'none'; payment 'none'; usb 'none'");
                    context.Response.Headers.Add("Referrer-Policy", "no-referrer-when-downgrade");
                    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                    context.Response.Headers.Add("X-Datacenter", _datacenter);
                    context.Response.Headers.Add("X-Download-Options", "noopen");
                    context.Response.Headers.Add("X-Frame-Options", "DENY");
                    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");

#if DEBUG
                    context.Response.Headers.Add("X-Debug", "true");
#endif

                    if (_environmentName != null)
                    {
                        context.Response.Headers.Add("X-Environment", _environmentName);
                    }

                    context.Response.Headers.Add("X-Instance", Environment.MachineName);
                    context.Response.Headers.Add("X-Request-Id", context.TraceIdentifier);
                    context.Response.Headers.Add("X-Revision", GitMetadata.Commit);

                    stopwatch.Stop();

                    string duration = stopwatch.Elapsed.TotalMilliseconds.ToString(
                        "0.00ms",
                        CultureInfo.InvariantCulture);

                    context.Response.Headers.Add("X-Request-Duration", duration);

                    return Task.CompletedTask;
                });

            await _next(context).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the Content Security Policy to use for the website.
        /// </summary>
        /// <param name="isProduction">Whether the current environment is production.</param>
        /// <param name="options">The current site configuration options.</param>
        /// <returns>
        /// A <see cref="string"/> containing the Content Security Policy to use.
        /// </returns>
        private static string BuildContentSecurityPolicy(bool isProduction, SiteOptions options)
        {
            string basePolicy = $@"
default-src 'self' stackpath.bootstrapcdn.com;
script-src 'self' ajax.googleapis.com cdnjs.cloudflare.com stackpath.bootstrapcdn.com www.google-analytics.com 'unsafe-inline';
style-src 'self' ajax.googleapis.com cdnjs.cloudflare.com fonts.googleapis.com stackpath.bootstrapcdn.com 'unsafe-inline';
img-src 'self' data: online.swagger.io www.google-analytics.com {GetCdnOriginForContentSecurityPolicy(options)};
font-src 'self' ajax.googleapis.com fonts.googleapis.com fonts.gstatic.com stackpath.bootstrapcdn.com;
connect-src 'self';
media-src 'none';
object-src 'none';
child-src 'none';
frame-ancestors 'none';
form-action 'self';
block-all-mixed-content;
base-uri https://api.martincostello.com;
manifest-src 'self';";

            var builder = new StringBuilder(basePolicy.Replace(Environment.NewLine, string.Empty, StringComparison.Ordinal).Replace("\n", string.Empty, StringComparison.Ordinal));

            if (isProduction)
            {
                builder.Append("upgrade-insecure-requests;");

                if (options?.ExternalLinks?.Reports?.ContentSecurityPolicy != null)
                {
                    builder.Append($"report-uri {options.ExternalLinks.Reports.ContentSecurityPolicy};");
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Gets the CDN origin to use for the Content Security Policy.
        /// </summary>
        /// <param name="options">The current site options.</param>
        /// <returns>
        /// The origin to use for the CDN, if any.
        /// </returns>
        private static string GetCdnOriginForContentSecurityPolicy(SiteOptions options)
        {
            return GetOriginForContentSecurityPolicy(options?.ExternalLinks?.Cdn);
        }

        /// <summary>
        /// Gets the origin to use for the Content Security Policy from the specified URI.
        /// </summary>
        /// <param name="baseUri">The base URI to get the origin for.</param>
        /// <returns>
        /// The origin to use for the URI, if any.
        /// </returns>
        private static string GetOriginForContentSecurityPolicy(Uri? baseUri)
        {
            if (baseUri == null)
            {
                return string.Empty;
            }

            var builder = new StringBuilder($"{baseUri.Host}");

            if (!baseUri.IsDefaultPort)
            {
                builder.Append($":{baseUri.Port}");
            }

            return builder.ToString();
        }
    }
}
