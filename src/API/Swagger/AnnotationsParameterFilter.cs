// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

#pragma warning disable CS1591
#pragma warning disable SA1600

using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MartinCostello.Api.Swagger;

public class AnnotationsParameterFilter : IParameterFilter
{
    public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
    {
        if (context.PropertyInfo is not null)
        {
            ApplyPropertyAnnotations(parameter, context.PropertyInfo);
        }
        else if (context.ParameterInfo is not null)
        {
            ApplyParamAnnotations(parameter, context.ParameterInfo);
        }
    }

    private static void ApplyPropertyAnnotations(OpenApiParameter parameter, PropertyInfo propertyInfo)
    {
        var attribute = propertyInfo
            .GetCustomAttributes<SwaggerParameterExampleAttribute>()
            .FirstOrDefault();

        if (attribute is not null)
        {
            ApplyExampleFromAttribute(parameter, attribute);
        }
    }

    private static void ApplyParamAnnotations(OpenApiParameter parameter, ParameterInfo parameterInfo)
    {
        var attribute = parameterInfo
            .GetCustomAttributes<SwaggerParameterExampleAttribute>()
            .FirstOrDefault();

        if (attribute is not null)
        {
            ApplyExampleFromAttribute(parameter, attribute);
        }
    }

    private static void ApplyExampleFromAttribute(OpenApiParameter parameter, SwaggerParameterExampleAttribute attribute)
    {
        if (attribute.Example is not null)
        {
            parameter.Example = ExampleFilter.FormatAsJson(attribute.Example);
        }
    }
}
