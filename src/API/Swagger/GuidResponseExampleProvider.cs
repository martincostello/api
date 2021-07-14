// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System;
using MartinCostello.Api.Models;

namespace MartinCostello.Api.Swagger
{
    /// <summary>
    /// A class representing an implementation of <see cref="IExampleProvider"/>
    /// for the <see cref="GuidResponse"/> class. This class cannot be inherited.
    /// </summary>
    public sealed class GuidResponseExampleProvider : IExampleProvider
    {
        /// <inheritdoc />
        public object GetExample()
        {
            return new GuidResponse()
            {
                Guid = Guid.NewGuid().ToString(),
            };
        }
    }
}
