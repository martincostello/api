// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System;
using System.IO;
using MartinCostello.Api.Options;
using MartinCostello.Api.Swagger;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MartinCostello.Api.Extensions
{
    /// <summary>
    /// A class containing extension methods for the <see cref="IServiceCollection"/> interface. This class cannot be inherited.
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Swagger to the services.
        /// </summary>
        /// <param name="value">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <param name="environment">The current hosting environment.</param>
        /// <returns>
        /// The value specified by <paramref name="value"/>.
        /// </returns>
        public static IServiceCollection AddSwagger(this IServiceCollection value, IWebHostEnvironment environment)
        {
            value.AddSwaggerGen((p) =>
                {
                    var provider = value.BuildServiceProvider();
                    var options = provider.GetRequiredService<IOptions<SiteOptions>>().Value;

                    var info = new OpenApiInfo()
                    {
                        Contact = new OpenApiContact()
                        {
                            Name = options.Metadata?.Author?.Name,
                            Url = new Uri(options.Metadata?.Author?.Website ?? string.Empty),
                        },
                        Description = options.Metadata?.Description,
                        License = new OpenApiLicense()
                        {
                            Name = options.Api?.License?.Name,
                            Url = new Uri(options.Api?.License?.Url ?? string.Empty),
                        },
                        Title = options.Metadata?.Name,
                        Version = string.Empty,
                    };

                    p.EnableAnnotations();

                    p.IgnoreObsoleteActions();
                    p.IgnoreObsoleteProperties();

                    AddXmlCommentsIfExists(p, environment, "API.xml");

                    p.SwaggerDoc("api", info);

                    p.SchemaFilter<ExampleFilter>();
                    p.OperationFilter<ExampleFilter>();
                    p.OperationFilter<RemoveStyleCopPrefixesFilter>();
                });

            return value;
        }

        /// <summary>
        /// Adds XML comments to Swagger if the file exists.
        /// </summary>
        /// <param name="options">The Swagger options.</param>
        /// <param name="environment">The current hosting environment.</param>
        /// <param name="fileName">The XML comments file name to try to add.</param>
        private static void AddXmlCommentsIfExists(SwaggerGenOptions options, IWebHostEnvironment environment, string fileName)
        {
            string applicationPath;

            if (environment.IsDevelopment())
            {
                applicationPath = Path.GetDirectoryName(typeof(Startup).Assembly.Location) ?? ".";
            }
            else
            {
                applicationPath = environment.ContentRootPath;
            }

            var path = Path.GetFullPath(Path.Combine(applicationPath, fileName));

            if (File.Exists(path))
            {
                options.IncludeXmlComments(path);
            }
        }
    }
}
