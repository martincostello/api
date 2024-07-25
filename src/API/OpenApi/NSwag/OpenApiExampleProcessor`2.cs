// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using NSwag.Generation.AspNetCore;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace MartinCostello.Api.OpenApi.NSwag;

/// <summary>
/// A class representing an processor for OpenAPI operation examples.
/// This class cannot be inherited.
/// </summary>
/// <typeparam name="TSchema">The type of the schema.</typeparam>
/// <typeparam name="TProvider">The type of the example provider.</typeparam>
public sealed class OpenApiExampleProcessor<TSchema, TProvider> : IOperationProcessor
    where TProvider : IExampleProvider<TSchema>
{
    /// <inheritdoc/>
    public bool Process(OperationProcessorContext context)
    {
        var examples = ((AspNetCoreOperationProcessorContext)context).ApiDescription.ActionDescriptor.EndpointMetadata
            .OfType<IOpenApiExampleMetadata>()
            .ToArray();

        foreach ((var info, var parameter) in context.Parameters)
        {
            if (parameter.Example is not null)
            {
                continue;
            }

            var metadata =
                GetExampleMetadata(info).FirstOrDefault() ??
                GetExampleMetadata(info.ParameterType).FirstOrDefault() ??
                examples.FirstOrDefault((p) => p.SchemaType == info.ParameterType);

            if (metadata is not null)
            {
                parameter.Example = CreateExample(metadata.GenerateExample());
            }
        }

        // Add examples for any schemas associated with the operation
        if (context.Document.Components.Schemas.TryGetValue(typeof(TSchema).Name, out var schema))
        {
            var example = TProvider.GenerateExample();

            // We cannot change ProblemDetails directly, so we need to adjust it if we see it
            if (example is ProblemDetails)
            {
                schema.AdditionalPropertiesSchema = NJsonSchema.JsonSchema.CreateAnySchema();
            }

            schema.Example = CreateExample(example);

            foreach (var parameter in context.OperationDescription.Operation.Parameters.Where((p) => p.Schema?.Reference == schema))
            {
                parameter.Example ??= schema.Example;
            }

            foreach (var response in context.OperationDescription.Operation.Responses.Values)
            {
                foreach (var mediaType in response.Content.Values.Where((p) => p.Schema?.Reference == schema))
                {
                    mediaType.Example ??= schema.Example;
                }
            }
        }

        return true;
    }

    private static JToken? CreateExample(object? example)
    {
        string? json = JsonSerializer.Serialize(example, example?.GetType() ?? typeof(TSchema), ApplicationJsonSerializerContext.Default);

        var serialized = JsonNode.Parse(json);

        if (serialized is null)
        {
            return null;
        }

        json = serialized.ToJsonString();
        return JToken.Parse(json);
    }

    private static IEnumerable<IOpenApiExampleMetadata> GetExampleMetadata(ParameterInfo parameter)
        => parameter.GetCustomAttributes().OfType<IOpenApiExampleMetadata>();

    private static IEnumerable<IOpenApiExampleMetadata> GetExampleMetadata(Type type)
        => type.GetCustomAttributes().OfType<IOpenApiExampleMetadata>();
}
