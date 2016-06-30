// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

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
