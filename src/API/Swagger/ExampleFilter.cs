// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MartinCostello.Api.Swagger
{
    /// <summary>
    /// A class representing an operation filter that adds the example to use for display in Swagger documentation. This class cannot be inherited.
    /// </summary>
    internal sealed class ExampleFilter : IOperationFilter, ISchemaFilter
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
            _options = options.Value.JsonSerializerOptions;
        }

        /// <inheritdoc />
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation != null && context?.ApiDescription != null && context.SchemaRepository != null)
            {
                if (!context.ApiDescription.TryGetMethodInfo(out MethodInfo methodInfo))
                {
                    return;
                }

                var examples = methodInfo
                    .GetCustomAttributes<SwaggerResponseExampleAttribute>(inherit: true)
                    .ToList();

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
        /// Creates an example from the specified type.
        /// </summary>
        /// <param name="exampleType">The type to create the example from.</param>
        /// <returns>
        /// The example value.
        /// </returns>
        private IOpenApiAny CreateExample(Type exampleType)
        {
            var provider = Activator.CreateInstance(exampleType) as IExampleProvider;
            return FormatAsJson(provider!);
        }

        /// <summary>
        /// Returns the example from the specified provider formatted as JSON.
        /// </summary>
        /// <param name="provider">The example provider to format the examples for.</param>
        /// <returns>
        /// An <see cref="object"/> representing the formatted example.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="provider"/> is <see langword="null"/>.
        /// </exception>
        private IOpenApiAny FormatAsJson(IExampleProvider provider)
        {
            var examples = provider.GetExample();

            // Apply any formatting rules configured for the API (e.g. camel casing)
            var json = JsonSerializer.Serialize(examples, _options);
            using var document = JsonDocument.Parse(json);

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
        /// Tries to parse the specified JSON token for an example.
        /// </summary>
        /// <param name="token">The JSON token to parse.</param>
        /// <param name="any">The the token was parsed, the <see cref="IOpenApiAny"/> to use.</param>
        /// <returns>
        /// <see langword="true"/> if parsed successfully; otherwise <see langword="false"/>.
        /// </returns>
        private bool TryParse(JsonElement token, out IOpenApiAny? any)
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
}
