// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace MartinCostello.Api.OpenApi.NSwag;

/// <summary>
/// A class representing a operation processor that removes
/// the position from parameters. This class cannot be inherited.
/// </summary>
public sealed class RemoveParameterPositionProcessor : IOperationProcessor
{
    /// <inheritdoc/>
    public bool Process(OperationProcessorContext context)
    {
        foreach (var parameter in context.Parameters.Values)
        {
            parameter.Position = null;

            if (parameter.Kind is global::NSwag.OpenApiParameterKind.Body)
            {
                parameter.Name = null;
            }
        }

        return true;
    }
}
