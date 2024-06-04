// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

#pragma warning disable CA1813

namespace MartinCostello.Api.OpenApi;

/// <summary>
/// Defines an attribute for an OpenAPI schema example.
/// </summary>
/// <typeparam name="TSchema">The type of the schema.</typeparam>
/// <typeparam name="TProvider">The type of the example provider.</typeparam>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class OpenApiExampleAttribute<TSchema, TProvider> : Attribute, IOpenApiOperationTransformer
    where TProvider : IExampleProvider<TSchema>
{
    /// <inheritdoc/>
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        JsonSerializerOptions? options = null;

        if (operation.Parameters is { Count: > 0 } parameters)
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
                    var example = argument.GetCustomAttribute<OpenApiParameterExampleAttribute>();
                    if (example?.Value is { } value)
                    {
                        var parameter = operation.Parameters.FirstOrDefault((p) => p.Name == argument.Name);
                        if (parameter is not null)
                        {
                            options ??= context.ApplicationServices.GetRequiredService<IOptions<JsonOptions>>().Value.SerializerOptions;
                            parameter.Example = FormatAsJson(value, options);
                        }
                    }
                }
            }
        }

        var schemaResponses = context.Description.SupportedResponseTypes
            .Where((p) => p.Type == typeof(TSchema))
            .ToArray();

        foreach (var schemaResponse in schemaResponses)
        {
            foreach (var responseFormat in schemaResponse.ApiResponseFormats)
            {
                foreach (var response in operation.Responses.Values)
                {
                    if (response.Content.TryGetValue(responseFormat.MediaType, out var mediaType) && mediaType.Example is null)
                    {
                        options ??= context.ApplicationServices.GetRequiredService<IOptions<JsonOptions>>().Value.SerializerOptions;
                        mediaType.Example = FormatAsJson(TProvider.GenerateExample(), options);
                    }
                }
            }
        }

        return Task.CompletedTask;
    }

    private static IOpenApiAny FormatAsJson<T>(T example, JsonSerializerOptions options)
    {
        // Apply any formatting rules configured for the API (e.g. camel casing)
        string? json = JsonSerializer.Serialize(example, options);
        using var document = JsonDocument.Parse(json);

        if (document.RootElement.ValueKind == JsonValueKind.String)
        {
            return new OpenApiString(document.RootElement.ToString());
        }

        var result = new OpenApiObject();

        // Recursively build up the example from the properties of the object
        foreach (var token in document.RootElement.EnumerateObject())
        {
            if (TryParse(token.Value, out var any))
            {
                result[token.Name] = any;
            }
        }

        return result;
    }

    private static bool TryParse(JsonElement token, out IOpenApiAny? any)
    {
        any = null;

        switch (token.ValueKind)
        {
            case JsonValueKind.Array:
                var array = new OpenApiArray();

                foreach (var value in token.EnumerateArray())
                {
                    if (TryParse(value, out var child))
                    {
                        array.Add(child);
                    }
                }

                any = array;
                return true;

            case JsonValueKind.False:
                any = new OpenApiBoolean(false);
                return true;

            case JsonValueKind.True:
                any = new OpenApiBoolean(true);
                return true;

            case JsonValueKind.Number:
                any = new OpenApiDouble(token.GetDouble());
                return true;

            case JsonValueKind.String:
                any = new OpenApiString(token.GetString());
                return true;

            case JsonValueKind.Object:
                var obj = new OpenApiObject();

                foreach (var child in token.EnumerateObject())
                {
                    if (TryParse(child.Value, out var value))
                    {
                        obj[child.Name] = value;
                    }
                }

                any = obj;
                return true;

            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
            default:
                return false;
        }
    }
}
