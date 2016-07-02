// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api.Extensions
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using Options;
    using Swagger;
    using Swashbuckle.Swagger.Model;

    /// <summary>
    /// A class containing extension methods for the <see cref="IServiceCollection"/> interface. This class cannot be inherited.
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Swagger to the services.
        /// </summary>
        /// <param name="value">The <see cref="IServiceCollection"/> to add the service to.</param>
        public static void AddSwagger(this IServiceCollection value)
        {
            value.AddSwaggerGen(
                (p) =>
                {
                    p.GroupActionsBy((r) => r.GroupName.ToLowerInvariant());
                    p.OrderActionGroupsBy(StringComparer.Ordinal);

                    p.DescribeAllEnumsAsStrings();
                    p.DescribeStringEnumsInCamelCase();

                    p.IgnoreObsoleteActions();
                    p.IgnoreObsoleteProperties();

                    var provider = value.BuildServiceProvider();

                    // Get the JSON formatter used by the API to use for formatting JSON in examples
                    p.OperationFilter<ExampleFilter>(provider.GetService<JsonSerializerSettings>());
                    p.OperationFilter<RemoveStyleCopPrefixesFilter>();

                    var options = provider.GetService<SiteOptions>();

                    p.SingleApiVersion(
                        new Info()
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
                            Version = "v1",
                        });
                });
        }
    }
}
