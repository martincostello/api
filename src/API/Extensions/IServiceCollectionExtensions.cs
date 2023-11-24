// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using MartinCostello.Api.Options;
using MartinCostello.Api.Swagger;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MartinCostello.Api.Extensions;

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
        value.AddSwaggerGen((options) =>
        {
            var provider = value.BuildServiceProvider();
            var siteOptions = provider.GetRequiredService<IOptions<SiteOptions>>().Value;

            var info = new OpenApiInfo()
            {
                Contact = new()
                {
                    Name = siteOptions.Metadata?.Author?.Name,
                    Url = new Uri(siteOptions.Metadata?.Author?.Website ?? string.Empty),
                },
                Description = siteOptions.Metadata?.Description,
                License = new()
                {
                    Name = siteOptions.Api?.License?.Name,
                    Url = new Uri(siteOptions.Api?.License?.Url ?? string.Empty),
                },
                Title = siteOptions.Metadata?.Name,
                Version = string.Empty,
            };

            options.EnableAnnotations();

            options.IgnoreObsoleteActions();
            options.IgnoreObsoleteProperties();

            AddXmlCommentsIfExists(options, environment, "API.xml");

            options.SwaggerDoc("api", info);

            options.SchemaFilter<ExampleFilter>();
            options.OperationFilter<ExampleFilter>();
            options.OperationFilter<RemoveStyleCopPrefixesFilter>();
            options.ParameterFilter<ExampleFilter>();
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
            applicationPath = Path.GetDirectoryName(AppContext.BaseDirectory) ?? ".";
        }
        else
        {
            applicationPath = environment.ContentRootPath;
        }

        string? path = Path.GetFullPath(Path.Combine(applicationPath, fileName));

        if (File.Exists(path))
        {
            options.IncludeXmlComments(path);
        }
    }
}
