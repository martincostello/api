﻿// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Reflection;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace MartinCostello.Api.OpenApi;

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
        foreach ((var info, var parameter) in context.Parameters)
        {
            if (info.GetCustomAttribute<OpenApiParameterExampleAttribute>() is { } example)
            {
                parameter.Example = example.Value;
            }
        }

        if (context.Document.Components.Schemas.TryGetValue(typeof(TSchema).Name, out var schema))
        {
            schema.Example = TProvider.GenerateExample();

            foreach (var parameter in context.OperationDescription.Operation.Parameters)
            {
                if (parameter.Schema?.Reference == schema)
                {
                    parameter.Example = schema.Example;
                }
            }

            foreach ((_, var response) in context.OperationDescription.Operation.Responses)
            {
                foreach (var mediaType in response.Content.Values)
                {
                    if (mediaType.Schema?.Reference == schema)
                    {
                        mediaType.Example = schema.Example;
                    }
                }
            }
        }

        return true;
    }
}
