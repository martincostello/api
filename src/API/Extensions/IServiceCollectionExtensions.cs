// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api.Extensions
{
    using System;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
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
        /// <param name="config">The current configuration.</param>
        public static void AddSwagger(this IServiceCollection value, IConfiguration config)
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

                    // Get the JSON formatter used by the API to use for formatting JSON in examples
                    p.OperationFilter<ExampleFilter>(value.BuildServiceProvider().GetService<JsonSerializerSettings>());
                    p.OperationFilter<RemoveStyleCopPrefixesFilter>();

                    p.SingleApiVersion(
                        new Info()
                        {
                            Contact = new Contact()
                            {
                                Email = config["Site:Metadata:Author:Email"],
                                Name = config["Site:Metadata:Author:Name"],
                                Url = config["Site:Metadata:Author:Website"],
                            },
                            Description = config["Site:Metadata:Description"],
                            License = new License()
                            {
                                Name = config["Site:Api:License:Name"],
                                Url = config["Site:Api:License:Url"],
                            },
                            TermsOfService = "https://github.com/martincostello/api/blob/master/LICENSE",
                            Title = config["Site:Metadata:Name"],
                            Version = "v1",
                        });
                });
        }
    }
}
