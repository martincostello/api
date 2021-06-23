// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using MartinCostello.Api.Models;

namespace MartinCostello.Api.Swagger
{
    /// <summary>
    /// A class representing an implementation of <see cref="IExampleProvider"/>
    /// for the <see cref="ErrorResponse"/> class. This class cannot be inherited.
    /// </summary>
    public sealed class ErrorResponseExampleProvider : IExampleProvider
    {
        /// <inheritdoc />
        public object GetExample()
        {
            return new ErrorResponse()
            {
                Message = "The specified value is invalid",
                RequestId = "0HKT0TM6UJASI",
                StatusCode = 400,
            };
        }
    }
}
