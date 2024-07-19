// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using ProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;

namespace MartinCostello.Api.OpenApi;

/// <summary>
/// A class representing an operation processor that adds examples to API endpoints. This class cannot be inherited.
/// </summary>
internal sealed class AddExamplesTransformer(IOptions<JsonOptions> options) : IOpenApiOperationTransformer, IOpenApiSchemaTransformer
{
    private readonly JsonSerializerOptions _options = options.Value.SerializerOptions;

    /// <inheritdoc />
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (operation.Parameters is { Count: > 0 } parameters)
        {
            TryAddParameterExamples(parameters, context);
        }

        var examples = context.Description.ActionDescriptor.EndpointMetadata
            .OfType<IOpenApiExampleMetadata>()
            .ToArray();

        if (examples.Length > 0)
        {
            if (operation.RequestBody is { } body)
            {
                TryAddRequestExamples(body, context, examples);
            }

            TryAddResponseExamples(operation.Responses, context, examples);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (context.JsonTypeInfo.Type == typeof(ProblemDetails))
        {
            schema.Example = ExampleFormatter.AsJson<ProblemDetails, ProblemDetailsExampleProvider>(_options);
        }
        else
        {
            var metadata = context.JsonTypeInfo.Type.GetCustomAttributes(false)
                .OfType<IOpenApiExampleMetadata>()
                .FirstOrDefault();

            if (metadata?.GenerateExample(_options) is { } value)
            {
                schema.Example = value;
            }
        }

        return Task.CompletedTask;
    }

    private void TryAddParameterExamples(
        IList<OpenApiParameter> parameters,
        OpenApiOperationTransformerContext context)
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

                if (metadata?.GenerateExample(_options) is { } value)
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

    private void TryAddRequestExamples(
        OpenApiRequestBody body,
        OpenApiOperationTransformerContext context,
        IList<IOpenApiExampleMetadata> examples)
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
            mediaType.Example = metadata.GenerateExample(_options);
        }
    }

    private void TryAddResponseExamples(
        OpenApiResponses responses,
        OpenApiOperationTransformerContext context,
        IList<IOpenApiExampleMetadata> examples)
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
                        mediaType.Example = examples.Single((p) => p.SchemaType == schemaResponse.Type).GenerateExample(_options);
                    }
                }
            }
        }
    }
}
