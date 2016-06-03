// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IExampleProvider.cs" company="https://martincostello.com/">
//   Martin Costello (c) 2016
// </copyright>
// <summary>
//   IExampleProvider.cs
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MartinCostello.Api.Swagger
{
    /// <summary>
    /// Defines a method for obtaining examples for Swagger documentation.
    /// </summary>
    internal interface IExampleProvider
    {
        /// <summary>
        /// Gets the example to use.
        /// </summary>
        /// <returns>
        /// An <see cref="object"/> that should be used as the example.
        /// </returns>
        object GetExample();
    }
}
