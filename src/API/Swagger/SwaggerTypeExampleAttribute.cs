// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System;

namespace MartinCostello.Api.Swagger
{
    /// <summary>
    /// Defines an example response for an API method. This class cannot be inherited.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    internal sealed class SwaggerTypeExampleAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SwaggerTypeExampleAttribute"/> class.
        /// </summary>
        /// <param name="exampleType">The type of the example.</param>
        public SwaggerTypeExampleAttribute(Type exampleType)
        {
            ExampleType = exampleType;
        }

        /// <summary>
        /// Gets the type of the example.
        /// </summary>
        public Type ExampleType { get; }
    }
}
