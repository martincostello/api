// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MartinCostello.Api.Swagger;

/// <summary>
/// A class representing an operation filter that adds the example to use for display in Swagger documentation. This class cannot be inherited.
/// </summary>
internal sealed class ExampleFilter : IOperationFilter, IParameterFilter, ISchemaFilter
{
    /// <summary>
    /// The <see cref="JsonSerializerOptions"/> to use for formatting example responses. This field is read-only.
    /// </summary>
    private readonly JsonSerializerOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExampleFilter"/> class.
    /// </summary>
    /// <param name="options">The <see cref="JsonOptions"/> to use.</param>
    public ExampleFilter(IOptions<JsonOptions> options)
    {
        _options = options.Value.SerializerOptions;
    }

    /// <inheritdoc />
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation != null && context?.ApiDescription != null && context.SchemaRepository != null)
        {
            AddRequestParameterExamples(operation, context);
            AddResponseExamples(operation, context);
        }
    }

    /// <inheritdoc />
    public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
    {
        if (context.PropertyInfo is not null)
        {
            ApplyPropertyAnnotations(parameter, context.PropertyInfo);
        }
        else if (context.ParameterInfo is not null)
        {
            ApplyParameterAnnotations(parameter, context.ParameterInfo);
        }
    }

    /// <inheritdoc />
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type != null)
        {
            var attribute = context.Type.GetCustomAttribute<SwaggerTypeExampleAttribute>();

            if (attribute != null)
            {
                schema.Example = CreateExample(attribute.ExampleType);
            }
        }
    }

    /// <summary>
    /// Gets all the attributes of the specified type associated with the API description.
    /// </summary>
    /// <typeparam name="T">The type of the attribute(s) to find.</typeparam>
    /// <param name="apiDescription">The API description.</param>
    /// <returns>
    /// An <see cref="IList{T}"/> containing any found attributes of type <typeparamref name="T"/>.
    /// </returns>
    private static IList<T> GetAttributes<T>(ApiDescription apiDescription)
        where T : Attribute
    {
        IEnumerable<T> attributes = Enumerable.Empty<T>();

        if (apiDescription.TryGetMethodInfo(out MethodInfo methodInfo))
        {
            attributes = attributes.Concat(methodInfo.GetCustomAttributes<T>(inherit: true));
        }

        if (apiDescription.ActionDescriptor is not null)
        {
            attributes = attributes.Concat(apiDescription.ActionDescriptor.EndpointMetadata.OfType<T>());
        }

        return attributes.ToArray();
    }

    /// <summary>
    /// Tries to parse the specified JSON token for an example.
    /// </summary>
    /// <param name="token">The JSON token to parse.</param>
    /// <param name="any">The the token was parsed, the <see cref="IOpenApiAny"/> to use.</param>
    /// <returns>
    /// <see langword="true"/> if parsed successfully; otherwise <see langword="false"/>.
    /// </returns>
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

    /// <summary>
    /// Creates an example from the specified type.
    /// </summary>
    /// <param name="exampleType">The type to create the example from.</param>
    /// <returns>
    /// The example value.
    /// </returns>
    private IOpenApiAny CreateExample(Type exampleType)
    {
        var provider = Activator.CreateInstance(exampleType) as IExampleProvider;
        object? examples = provider!.GetExample();

        return FormatAsJson(examples);
    }

    /// <summary>
    /// Returns the example from the specified provider formatted as JSON.
    /// </summary>
    /// <param name="examples">The examples to format.</param>
    /// <returns>
    /// An <see cref="object"/> representing the formatted example.
    /// </returns>
    private IOpenApiAny FormatAsJson(object? examples)
    {
        // Apply any formatting rules configured for the API (e.g. camel casing)
        string? json = JsonSerializer.Serialize(examples, _options);
        using var document = JsonDocument.Parse(json);

        if (document.RootElement.ValueKind == JsonValueKind.String)
        {
            return new OpenApiString(document.RootElement.ToString());
        }

        var result = new OpenApiObject();

        // Recursively build up the example from the properties of the JObject
        foreach (var token in document.RootElement.EnumerateObject())
        {
            if (TryParse(token.Value, out var any))
            {
                result[token.Name] = any;
            }
        }

        return result;
    }

    /// <summary>
    /// Adds the request parameter examples.
    /// </summary>
    /// <param name="operation">The operation to add the examples for.</param>
    /// <param name="context">The operation context.</param>
    private void AddRequestParameterExamples(OpenApiOperation operation, OperationFilterContext context)
    {
        var examples = GetAttributes<SwaggerRequestExampleAttribute>(context.ApiDescription);

        foreach (var attribute in examples)
        {
            if (!context.SchemaRepository.TryLookupByType(attribute.RequestType, out OpenApiSchema schema) ||
                !context.SchemaRepository.Schemas.TryGetValue(schema.Reference.Id, out OpenApiSchema _))
            {
                continue;
            }

            var request = operation.RequestBody.Content.First();

            if (string.Equals(request.Value.Schema.Reference.Id, schema.Reference.Id, StringComparison.Ordinal))
            {
                request.Value.Example = CreateExample(attribute.ExampleType);
            }
        }
    }

    /// <summary>
    /// Adds the response examples.
    /// </summary>
    /// <param name="operation">The operation to add the examples for.</param>
    /// <param name="context">The operation context.</param>
    private void AddResponseExamples(OpenApiOperation operation, OperationFilterContext context)
    {
        var examples = GetAttributes<SwaggerResponseExampleAttribute>(context.ApiDescription);

        foreach (var attribute in examples)
        {
            if (!context.SchemaRepository.Schemas.TryGetValue(attribute.ResponseType.Name, out var schema))
            {
                continue;
            }

            var response = operation.Responses
                .SelectMany((p) => p.Value.Content)
                .Where((p) => p.Value.Schema.Reference.Id == attribute.ResponseType.Name)
                .Select((p) => p)
                .FirstOrDefault();

            if (!response.Equals(new KeyValuePair<string, OpenApiMediaType>()))
            {
                response.Value.Example = CreateExample(attribute.ExampleType);
            }
        }
    }

    /// <summary>
    /// Applies any relevant property annotations.
    /// </summary>
    /// <param name="parameter">The parameter to annotate.</param>
    /// <param name="propertyInfo">The property being annotated.</param>
    private void ApplyPropertyAnnotations(OpenApiParameter parameter, PropertyInfo propertyInfo)
    {
        var attribute = propertyInfo
            .GetCustomAttributes<SwaggerParameterExampleAttribute>()
            .FirstOrDefault();

        if (attribute is not null)
        {
            ApplyExampleFromAttribute(parameter, attribute);
        }
    }

    /// <summary>
    /// Applies any relevant parameter annotations.
    /// </summary>
    /// <param name="parameter">The parameter to annotate.</param>
    /// <param name="parameterInfo">The parameter being annotated.</param>
    private void ApplyParameterAnnotations(OpenApiParameter parameter, ParameterInfo parameterInfo)
    {
        var attribute = parameterInfo
            .GetCustomAttributes<SwaggerParameterExampleAttribute>()
            .FirstOrDefault();

        if (attribute is not null)
        {
            ApplyExampleFromAttribute(parameter, attribute);
        }
    }

    /// <summary>
    /// Applies the example to the specified parameter.
    /// </summary>
    /// <param name="parameter">The parameter to apply the example to.</param>
    /// <param name="attribute">The attribute declaring the example.</param>
    private void ApplyExampleFromAttribute(OpenApiParameter parameter, SwaggerParameterExampleAttribute attribute)
    {
        if (attribute.Example is not null)
        {
            parameter.Example = FormatAsJson(attribute.Example);
        }
    }
}
