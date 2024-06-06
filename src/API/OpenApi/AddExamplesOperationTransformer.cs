﻿// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace MartinCostello.Api.OpenApi;

/// <summary>
/// A class representing an operation processor that adds examples to API endpoints. This class cannot be inherited.
/// </summary>
internal sealed class AddExamplesOperationTransformer : IOpenApiOperationTransformer
{
    /// <inheritdoc />
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var options = context.ApplicationServices.GetRequiredService<IOptions<JsonOptions>>().Value.SerializerOptions;

        if (operation.Parameters is { Count: > 0 } parameters)
        {
            TryAddParameterExamples(parameters, context, options);
        }

        var examples = context.Description.ActionDescriptor.EndpointMetadata
            .OfType<IOpenApiExampleMetadata>()
            .ToArray();

        if (examples.Length > 0)
        {
            if (operation.RequestBody is { } body)
            {
                TryAddRequestExamples(body, context, examples, options);
            }

            TryAddResponseExamples(operation.Responses, context, examples, options);
        }

        return Task.CompletedTask;
    }

    private static void TryAddParameterExamples(
        IList<OpenApiParameter> parameters,
        OpenApiOperationTransformerContext context,
        JsonSerializerOptions options)
    {
        var methodInfo = context.Description.ActionDescriptor.EndpointMetadata
            .OfType<MethodInfo>()
            .FirstOrDefault();

        var arguments = methodInfo?
            .GetParameters()
            .ToArray();

        if (arguments is { Length: > 0 })
        {
            foreach (var argument in arguments)
            {
                var metadata = argument.GetCustomAttributes()
                    .OfType<IOpenApiExampleMetadata>()
                    .FirstOrDefault((p) => p.SchemaType == argument.ParameterType);

                if (metadata?.GenerateExample(options) is { } value)
                {
                    var parameter = parameters.FirstOrDefault((p) => p.Name == argument.Name);
                    if (parameter is not null)
                    {
                        parameter.Example = value;
                    }
                }
            }
        }
    }

    private static void TryAddRequestExamples(
        OpenApiRequestBody body,
        OpenApiOperationTransformerContext context,
        IList<IOpenApiExampleMetadata> examples,
        JsonSerializerOptions options)
    {
        var schemaResponses = context.Description.ParameterDescriptions
            .Where((p) => p.Source == BindingSource.Body)
            .Where((p) => examples.Any((r) => r.SchemaType == p.Type))
            .ToArray();

        if (schemaResponses.Length < 1)
        {
            return;
        }

        var metadata = examples.FirstOrDefault((p) => p.SchemaType == schemaResponses[0].Type);

        if (metadata is not null && body.Content.TryGetValue("application/json", out var mediaType))
        {
            mediaType.Example = metadata.GenerateExample(options);
        }
    }

    private static void TryAddResponseExamples(
        OpenApiResponses responses,
        OpenApiOperationTransformerContext context,
        IList<IOpenApiExampleMetadata> examples,
        JsonSerializerOptions options)
    {
        var schemaResponses = context.Description.SupportedResponseTypes
            .Where((p) => examples.Any((r) => r.SchemaType == p.Type))
            .ToArray();

        if (schemaResponses.Length < 1)
        {
            return;
        }

        foreach (var schemaResponse in schemaResponses)
        {
            foreach (var responseFormat in schemaResponse.ApiResponseFormats)
            {
                foreach (var response in responses.Values)
                {
                    if (response.Content.TryGetValue(responseFormat.MediaType, out var mediaType) && mediaType.Example is null)
                    {
                        mediaType.Example = examples.Single((p) => p.SchemaType == schemaResponse.Type).GenerateExample(options);
                    }
                }
            }
        }
    }
}