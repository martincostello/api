// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Reflection;
using MartinCostello.Api.OpenApi;
using MartinCostello.Api.Options;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

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
        services.AddOpenApi("api", (options) =>
        {
            options.UseTransformer<ApiInfoTransformer>();
            options.UseTransformer<OperationExampleTransformer>();
            options.UseTransformer<OperationResponseTransformer>();
            options.UseTransformer<RemoveStyleCopPrefixesProcessor>();
            options.UseTransformer<UpdateProblemDetailsMediaTypeTransformer>();

            options.UseOperationTransformer(OperationTransformers.AddResponseExamples);
        });

        return services;
    }

    private sealed class ApiInfoTransformer(IOptions<SiteOptions> options) : IOpenApiDocumentTransformer
    {
        public Task TransformAsync(
            OpenApiDocument document,
            OpenApiDocumentTransformerContext context,
            CancellationToken cancellationToken)
        {
            ConfigureInfo(document.Info);

            return Task.CompletedTask;
        }

        private void ConfigureInfo(OpenApiInfo info)
        {
            var siteOptions = options.Value;

            info.Description = siteOptions.Metadata?.Description;
            info.Title = siteOptions.Metadata?.Name;
            info.Version = string.Empty;

            info.Contact = new()
            {
                Name = siteOptions.Metadata?.Author?.Name,
            };

            if (siteOptions.Metadata?.Author?.Website is { } contactUrl)
            {
                info.Contact.Url = new(contactUrl);
            }

            info.License = new()
            {
                Name = siteOptions.Api?.License?.Name,
            };

            if (siteOptions.Api?.License?.Url is { } licenseUrl)
            {
                info.License.Url = new(licenseUrl);
            }
        }
    }

    private sealed class OperationExampleTransformer : IOpenApiDocumentTransformer
    {
        public async Task TransformAsync(
            OpenApiDocument document,
            OpenApiDocumentTransformerContext context,
            CancellationToken cancellationToken)
        {
            foreach ((string path, var item) in document.Paths)
            {
                foreach ((var httpMethod, var operation) in item.Operations)
                {
                    var endpoint = context.DescriptionGroups
                        .SelectMany((p) => p.Items)
                        .Where((p) => string.Equals(p.HttpMethod, httpMethod.ToString(), StringComparison.OrdinalIgnoreCase))
                        .FirstOrDefault((p) => path.AsSpan(1).SequenceEqual(p.RelativePath));

                    if (endpoint is null)
                    {
                        continue;
                    }

                    var transformers = endpoint.ActionDescriptor.EndpointMetadata
                        .OfType<IOpenApiDocumentTransformer>()
                        .ToArray();

                    foreach (var transformer in transformers)
                    {
                        await transformer.TransformAsync(document, context, cancellationToken);
                    }

                    if (operation.Parameters?.Count > 0)
                    {
                        var method = endpoint.ActionDescriptor.EndpointMetadata
                            .OfType<MethodInfo>()
                            .FirstOrDefault();

                        // Get all the arguments for the method
                        var arguments = method?.GetParameters().ToArray();

                        if (arguments is not null)
                        {
                            foreach (var arg in arguments)
                            {
                                var custom = arg.GetCustomAttribute<OpenApiParameterExampleAttribute>();
                                if (custom?.Value is { Length: > 0 } example)
                                {
                                    var parameter = operation.Parameters.FirstOrDefault((p) => p.Name == arg.Name);
                                    if (parameter is not null)
                                    {
                                        parameter.Example = new OpenApiString(example);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private sealed class OperationResponseTransformer : IOpenApiDocumentTransformer
    {
        public Task TransformAsync(
            OpenApiDocument document,
            OpenApiDocumentTransformerContext context,
            CancellationToken cancellationToken)
        {
            foreach ((string path, var item) in document.Paths)
            {
                foreach ((var httpMethod, var operation) in item.Operations)
                {
                    var endpoint = context.DescriptionGroups
                        .SelectMany((p) => p.Items)
                        .Where((p) => string.Equals(p.HttpMethod, httpMethod.ToString(), StringComparison.OrdinalIgnoreCase))
                        .FirstOrDefault((p) => path.AsSpan(1).SequenceEqual(p.RelativePath));

                    if (endpoint is null)
                    {
                        continue;
                    }

                    var responses = endpoint.ActionDescriptor.EndpointMetadata.OfType<OpenApiResponseAttribute>().ToArray();

                    foreach (var attribute in responses)
                    {
                        if (operation.Responses.TryGetValue(attribute.HttpStatusCode.ToString(CultureInfo.InvariantCulture), out var response))
                        {
                            response.Description = attribute.Description;
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// A class representing a document processor that removes StyleCop
    /// prefixes from property descriptions. This class cannot be inherited.
    /// </summary>
    private sealed class RemoveStyleCopPrefixesProcessor : IOpenApiDocumentTransformer
    {
        private const string Prefix = "Gets or sets ";

        /// <inheritdoc/>
        public Task TransformAsync(
            OpenApiDocument document,
            OpenApiDocumentTransformerContext context,
            CancellationToken cancellationToken)
        {
            if (document.Components?.Schemas is { } schemas)
            {
                foreach (var schema in schemas)
                {
                    foreach (var property in schema.Value.Properties.Values)
                    {
                        TryUpdateDescription(property);
                    }
                }
            }

            foreach (var path in document.Paths.Values)
            {
                foreach (var operation in path.Operations.Values)
                {
                    foreach (var response in operation.Responses.Values)
                    {
                        foreach (var model in response.Content.Values)
                        {
                            foreach (var property in model.Schema.Properties.Values)
                            {
                                TryUpdateDescription(property);
                            }
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }

        private static void TryUpdateDescription(OpenApiSchema property)
        {
            if (property.Description is not null)
            {
                property.Description = property.Description.Replace(Prefix, string.Empty, StringComparison.Ordinal);
                property.Description = char.ToUpperInvariant(property.Description[0]) + property.Description[1..];
            }
        }
    }

    /// <summary>
    /// A class representing a operation processor that fixes the media type
    /// to use for <see cref="ProblemDetails"/>. This class cannot be inherited.
    /// </summary>
    private sealed class UpdateProblemDetailsMediaTypeTransformer : IOpenApiDocumentTransformer
    {
        /// <inheritdoc/>
        public Task TransformAsync(
            OpenApiDocument document,
            OpenApiDocumentTransformerContext context,
            CancellationToken cancellationToken)
        {
            foreach (var path in document.Paths.Values)
            {
                foreach (var operation in path.Operations.Values)
                {
                    foreach ((string status, var response) in operation.Responses)
                    {
                        if (status.StartsWith('2'))
                        {
                            continue;
                        }

                        if (response.Content.TryGetValue("application/json", out var mediaType))
                        {
                            response.Content["application/problem+json"] = mediaType;
                            response.Content.Remove("application/json");
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
