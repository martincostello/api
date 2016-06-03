// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SwaggerResponseExampleAttribute.cs" company="https://martincostello.com/">
//   Martin Costello (c) 2016
// </copyright>
// <summary>
//   SwaggerResponseExampleAttribute.cs
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MartinCostello.Api.Swagger
{
    using System;

    /// <summary>
    /// Defines an example response for an API method. This class cannot be inherited.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    internal sealed class SwaggerResponseExampleAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SwaggerResponseExampleAttribute"/> class.
        /// </summary>
        /// <param name="responseType">The type of the response.</param>
        /// <param name="exampleType">The type of the example.</param>
        public SwaggerResponseExampleAttribute(Type responseType, Type exampleType)
        {
            ResponseType = responseType;
            ExampleType = exampleType;
        }

        /// <summary>
        /// Gets the type of the response.
        /// </summary>
        public Type ResponseType { get; }

        /// <summary>
        /// Gets the type of the example.
        /// </summary>
        public Type ExampleType { get; }
    }
}
