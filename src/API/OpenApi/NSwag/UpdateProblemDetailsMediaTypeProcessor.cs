// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace MartinCostello.Api.OpenApi.NSwag;

/// <summary>
/// A class representing a operation processor that fixes the media type
/// to use for <see cref="ProblemDetails"/>. This class cannot be inherited.
/// </summary>
public sealed class UpdateProblemDetailsMediaTypeProcessor : IOperationProcessor
{
    /// <inheritdoc/>
    public bool Process(OperationProcessorContext context)
    {
        foreach ((string status, var response) in context.OperationDescription.Operation.Responses)
        {
            if (!status.StartsWith('4'))
            {
                continue;
            }

            foreach ((string key, var mediaType) in response.Content.ToImmutableDictionary())
            {
                if (key is "application/json")
                {
                    response.Content["application/problem+json"] = mediaType;
                    response.Content.Remove(key);
                }
            }
        }

        return true;
    }
}
