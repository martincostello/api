﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomHttpHeadersMiddleware.cs" company="https://martincostello.com/">
//   Martin Costello (c) 2016
// </copyright>
// <summary>
//   CustomHttpHeadersMiddleware.cs
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MartinCostello.Api.Middleware
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;

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
        /// The current Content Security Policy. This field is read-only.
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
        public CustomHttpHeadersMiddleware(RequestDelegate next, IHostingEnvironment environment, IConfiguration config)
        {
            _next = next;
            _isProduction = environment.IsProduction();
            _environmentName = _isProduction ? null : environment.EnvironmentName;
            _datacenter = config["Azure:Datacenter"] ?? "Local";
            _contentSecurityPolicy = BuildContentSecurityPolicy(_isProduction);
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

                    if (_isProduction)
                    {
                        context.Response.Headers.Add("Arr-Disable-Session-Affinity", bool.TrueString);
                    }

                    context.Response.Headers.Add("Content-Security-Policy", _contentSecurityPolicy);
                    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                    context.Response.Headers.Add("X-Download-Options", "noopen");
                    context.Response.Headers.Add("X-Frame-Options", "DENY");
                    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");

                    if (context.Request.IsHttps)
                    {
                        context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000");
                    }

                    context.Response.Headers.Add("X-Datacenter", _datacenter);

                    if (_environmentName != null)
                    {
                        context.Response.Headers.Add("X-Environment", _environmentName);
                    }

                    context.Response.Headers.Add("X-Instance", Environment.MachineName);
                    context.Response.Headers.Add("X-Request-Id", context.TraceIdentifier);

                    stopwatch.Stop();

                    string duration = stopwatch.Elapsed.TotalMilliseconds.ToString(
                        "0.00ms",
                        CultureInfo.InvariantCulture);

                    context.Response.Headers.Add("X-Request-Duration", duration);

                    return Task.CompletedTask;
                });

            await _next(context);
        }

        /// <summary>
        /// Builds the Content Security Policy to use for the website.
        /// </summary>
        /// <param name="isProduction">Whether the current environment is production.</param>
        /// <returns>
        /// A <see cref="string"/> containing the Content Security Policy to use.
        /// </returns>
        private static string BuildContentSecurityPolicy(bool isProduction)
        {
            const string BasePolicy = @"
default-src 'self';
script-src 'self' ajax.googleapis.com maxcdn.bootstrapcdn.com www.google-analytics.com 'unsafe-inline';
style-src 'self' ajax.googleapis.com fonts.googleapis.com maxcdn.bootstrapcdn.com 'unsafe-inline';
img-src 'self' online.swagger.io www.google-analytics.com;
font-src 'self' ajax.googleapis.com fonts.googleapis.com fonts.gstatic.com maxcdn.bootstrapcdn.com;
connect-src 'self';
media-src 'none';
object-src 'none';
child-src 'none';
frame-ancestors 'none';
form-action 'self';
block-all-mixed-content;
reflected-xss block;
base-uri https://api.martincostello.com;
manifest-src 'self';";

            var builder = new StringBuilder(BasePolicy.Replace(Environment.NewLine, string.Empty));

            if (isProduction)
            {
                builder.Append("upgrade-insecure-requests;");
            }

            return builder.ToString();
        }
    }
}
