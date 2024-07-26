// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace MartinCostello.Api.OpenApi;

/// <summary>
/// A class representing an operation processor that adds examples to API endpoints. This class cannot be inherited.
/// </summary>
internal sealed class AddExamplesTransformer : IOpenApiOperationTransformer, IOpenApiSchemaTransformer
{
    private static readonly ApplicationJsonSerializerContext Context = ApplicationJsonSerializerContext.Default;

    /// <inheritdoc />
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        Process(operation, context.Description);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        Type? type = null; // context.JsonTypeInfo.Type;

#pragma warning disable CA1508
        if (type is not null)
#pragma warning restore CA1508
        {
            Process(schema, type);
        }

        return Task.CompletedTask;
    }

    private static void Process(OpenApiOperation operation, ApiDescription description)
    {
        var examples = description.ActionDescriptor.EndpointMetadata
            .OfType<IOpenApiExampleMetadata>()
            .ToArray();

        if (operation.Parameters is { Count: > 0 } parameters)
        {
            TryAddParameterExamples(parameters, description, examples);
        }

        if (operation.RequestBody is { } body)
        {
            TryAddRequestExamples(body, description, examples);
        }

        TryAddResponseExamples(operation.Responses, description, examples);
    }

    private static void Process(OpenApiSchema schema, Type type)
    {
        if (schema.Example is not null)
        {
            return;
        }

        if (type == typeof(ProblemDetails))
        {
            schema.Example = ExampleFormatter.AsJson<ProblemDetails, ProblemDetailsExampleProvider>(Context);
        }
        else if (GetExampleMetadata(type).FirstOrDefault() is { } metadata)
        {
            schema.Example = metadata.GenerateExample(Context);
        }
    }

    private static void TryAddParameterExamples(
        IList<OpenApiParameter> parameters,
        ApiDescription description,
        IList<IOpenApiExampleMetadata> examples)
    {
        var arguments = description.ActionDescriptor.EndpointMetadata
            .OfType<MethodInfo>()
            .FirstOrDefault()?
            .GetParameters()
            .ToArray();

        if (arguments is { Length: > 0 })
        {
            foreach (var argument in arguments)
            {
                var metadata =
                    GetExampleMetadata(argument).FirstOrDefault((p) => p.SchemaType == argument.ParameterType) ??
                    GetExampleMetadata(argument.ParameterType).FirstOrDefault((p) => p.SchemaType == argument.ParameterType) ??
                    examples.FirstOrDefault((p) => p.SchemaType == argument.ParameterType);

                if (metadata?.GenerateExample(Context) is { } value)
                {
                    var parameter = parameters.FirstOrDefault((p) => p.Name == argument.Name);
                    if (parameter is not null)
                    {
                        parameter.Example ??= value;
                    }
                }
            }
        }
    }

    private static void TryAddRequestExamples(
        OpenApiRequestBody body,
        ApiDescription description,
        IList<IOpenApiExampleMetadata> examples)
    {
        if (!body.Content.TryGetValue("application/json", out var mediaType) || mediaType.Example is not null)
        {
            return;
        }

        var bodyParameter = description.ParameterDescriptions.Single((p) => p.Source == BindingSource.Body);

        var metadata =
            GetExampleMetadata(bodyParameter.Type).FirstOrDefault() ??
            examples.FirstOrDefault((p) => p.SchemaType == bodyParameter.Type);

        if (metadata is not null)
        {
            mediaType.Example ??= metadata.GenerateExample(Context);
        }
    }

    private static void TryAddResponseExamples(
        OpenApiResponses responses,
        ApiDescription description,
        IList<IOpenApiExampleMetadata> examples)
    {
        foreach (var schemaResponse in description.SupportedResponseTypes)
        {
            schemaResponse.Type ??= schemaResponse.ModelMetadata?.ModelType;

            var metadata = GetExampleMetadata(schemaResponse.Type)?
                .FirstOrDefault((p) => p.SchemaType == schemaResponse.Type);

            foreach (var responseFormat in schemaResponse.ApiResponseFormats)
            {
                if (responses.TryGetValue(schemaResponse.StatusCode.ToString(CultureInfo.InvariantCulture), out var response) &&
                    response.Content.TryGetValue(responseFormat.MediaType, out var mediaType))
                {
                    mediaType.Example ??= (metadata ?? examples.SingleOrDefault((p) => p.SchemaType == schemaResponse.Type))?.GenerateExample(Context);
                }
            }
        }
    }

    private static IEnumerable<IOpenApiExampleMetadata> GetExampleMetadata(ParameterInfo parameter)
        => parameter.GetCustomAttributes().OfType<IOpenApiExampleMetadata>();

    private static IEnumerable<IOpenApiExampleMetadata> GetExampleMetadata(Type? type)
        => type?.GetCustomAttributes().OfType<IOpenApiExampleMetadata>() ?? [];
}
