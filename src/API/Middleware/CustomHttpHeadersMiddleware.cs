// --------------------------------------------------------------------------------------------------------------------
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
    using System.Threading.Tasks;
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
        /// The name of the current hosting environment. This field is read-only.
        /// </summary>
        private readonly string _environmentName;

        /// <summary>
        /// The current datacenter name. This field is read-only.
        /// </summary>
        private readonly string _datacenter;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomHttpHeadersMiddleware"/> class.
        /// </summary>
        /// <param name="next">The delegate for the next part of the pipeline.</param>
        /// <param name="environmentName">The name of the current hosting environment.</param>
        /// <param name="config">The current configuration.</param>
        public CustomHttpHeadersMiddleware(RequestDelegate next, string environmentName, IConfiguration config)
        {
            _next = next;
            _environmentName = environmentName;
            _datacenter = config["Azure:Datacenter"] ?? "Local";
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

                    context.Response.Headers.Add("Arr-Disable-Session-Affinity", bool.TrueString);

                    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                    context.Response.Headers.Add("X-Frame-Options", "DENY");
                    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");

                    context.Response.Headers.Add("X-Datacenter", _datacenter);
                    context.Response.Headers.Add("X-Environment", _environmentName);
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
    }
}
