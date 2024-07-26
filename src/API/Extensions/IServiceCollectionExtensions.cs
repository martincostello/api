// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using MartinCostello.Api.OpenApi;

namespace MartinCostello.Api.Extensions;

/// <summary>
/// A class containing extension methods for the <see cref="IServiceCollection"/> interface. This class cannot be inherited.
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds OpenAPI documentation to the services.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add OpenAPI documentation to.</param>
    /// <returns>
    /// The value specified by <paramref name="services"/>.
    /// </returns>
    public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<AddExamplesTransformer>();

        services.AddOpenApi("api", (options) =>
        {
            options.AddDocumentTransformer<AddApiInfoTransformer>();
            options.AddDocumentTransformer<AddServersTransformer>();

            options.AddOperationTransformer(new AddResponseDescriptionTransformer());
            options.AddSchemaTransformer(new AddSchemaDescriptionsTransformer());

            var examples = new AddExamplesTransformer();
            options.AddOperationTransformer(examples);
            options.AddSchemaTransformer(examples);

            var prefixes = new RemoveStyleCopPrefixesTransformer();
            options.AddOperationTransformer(prefixes);
            options.AddSchemaTransformer(prefixes);
        });

        return services;
    }
}
