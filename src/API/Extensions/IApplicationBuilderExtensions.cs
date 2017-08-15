// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api.Extensions
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Middleware;
    using Options;

    /// <summary>
    /// A class containing extension methods for the <see cref="IApplicationBuilder"/> interface. This class cannot be inherited.
    /// </summary>
    public static class IApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds the custom HTTP headers middleware to the pipeline.
        /// </summary>
        /// <param name="value">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
        /// <param name="environment">The current hosting environment.</param>
        /// <param name="config">The current configuration.</param>
        /// <param name="options">The current site configuration options.</param>
        /// <returns>
        /// The value specified by <paramref name="value"/>.
        /// </returns>
        public static IApplicationBuilder UseCustomHttpHeaders(
            this IApplicationBuilder value,
            IHostingEnvironment environment,
            IConfiguration config,
            SiteOptions options)
        {
            return value.UseMiddleware<CustomHttpHeadersMiddleware>(environment, config, options);
        }

        /// <summary>
        /// Adds Swagger to the pipeline.
        /// </summary>
        /// <param name="value">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
        /// <param name="options">The site options.</param>
        /// <returns>
        /// The value specified by <paramref name="value"/>.
        /// </returns>
        public static IApplicationBuilder UseSwagger(this IApplicationBuilder value, SiteOptions options)
        {
            return value
                .UseSwagger()
                .UseSwaggerUI(
                    (p) =>
                    {
                        p.RoutePrefix = options.Api.Documentation.Location;
                        p.ShowJsonEditor();
                        p.ShowRequestHeaders();
                        p.SwaggerEndpoint($"/swagger/api/swagger.json", options.Metadata.Name);
                    });
        }
    }
}
