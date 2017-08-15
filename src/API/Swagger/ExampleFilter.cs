// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api.Swagger
{
    using System;
    using System.Linq;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Swagger;
    using Swashbuckle.AspNetCore.SwaggerGen;

    /// <summary>
    /// A class representing an operation filter that adds the example to use for display in Swagger documentation. This class cannot be inherited.
    /// </summary>
    internal sealed class ExampleFilter : IOperationFilter
    {
        /// <summary>
        /// The <see cref="JsonSerializerSettings"/> to use for formatting example responses. This field is read-only.
        /// </summary>
        private readonly JsonSerializerSettings _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExampleFilter"/> class.
        /// </summary>
        /// <param name="settings">The <see cref="JsonSerializerSettings"/> to use.</param>
        public ExampleFilter(JsonSerializerSettings settings)
        {
            _settings = settings;
        }

        /// <inheritdoc />
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (operation != null && context?.ApiDescription != null && context.SchemaRegistry != null)
            {
                var responseAttributes = context.ApiDescription.ActionAttributes()
                    .OfType<SwaggerResponseExampleAttribute>()
                    .ToList();

                foreach (var attribute in responseAttributes)
                {
                    var schema = context.SchemaRegistry.GetOrRegister(attribute.ResponseType);

                    var response = operation.Responses
                        .Where((p) => p.Value.Schema?.Type == schema.Type)
                        .Where((p) => p.Value.Schema?.Ref == schema.Ref)
                        .Select((p) => p.Value)
                        .FirstOrDefault();

                    if (response != null)
                    {
                        var provider = (IExampleProvider)Activator.CreateInstance(attribute.ExampleType);
                        response.Examples = FormatAsJson(provider);
                    }
                }
            }
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
        private object FormatAsJson(IExampleProvider provider)
        {
            var examples = provider.GetExample();

            // Apply any formatting rules configured for the API (e.g. camel casing)
            var json = JsonConvert.SerializeObject(examples, _settings);
            return JsonConvert.DeserializeObject(json);
        }
    }
}
