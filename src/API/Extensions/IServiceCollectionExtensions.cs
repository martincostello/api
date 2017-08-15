// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api.Extensions
{
    using System.IO;
    using System.Reflection;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Options;
    using Swagger;
    using Swashbuckle.AspNetCore.Swagger;
    using Swashbuckle.AspNetCore.SwaggerGen;

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
        public static IServiceCollection AddSwagger(this IServiceCollection value, IHostingEnvironment environment)
        {
            value.AddSwaggerGen((p) =>
                {
                    var provider = value.BuildServiceProvider();
                    var options = provider.GetRequiredService<SiteOptions>();

                    var info = new Info()
                    {
                        Contact = new Contact()
                        {
                            Email = options.Metadata.Author.Email,
                            Name = options.Metadata.Author.Name,
                            Url = options.Metadata.Author.Website,
                        },
                        Description = options.Metadata.Description,
                        License = new License()
                        {
                            Name = options.Api.License.Name,
                            Url = options.Api.License.Url,
                        },
                        TermsOfService = options.Metadata.Repository + "/blob/master/LICENSE",
                        Title = options.Metadata.Name,
                        Version = string.Empty,
                    };

                    p.DescribeAllEnumsAsStrings();
                    p.DescribeStringEnumsInCamelCase();

                    p.IgnoreObsoleteActions();
                    p.IgnoreObsoleteProperties();

                    AddXmlCommentsIfExists(p, environment, "API.xml");

                    p.SwaggerDoc("api", info);

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
        private static void AddXmlCommentsIfExists(SwaggerGenOptions options, IHostingEnvironment environment, string fileName)
        {
            var modelType = typeof(Startup).GetTypeInfo();
            string applicationPath;

            if (environment.IsDevelopment())
            {
                applicationPath = Path.GetDirectoryName(modelType.Assembly.Location);
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
