// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using MartinCostello.Api.Models;

namespace MartinCostello.Api.Swagger
{
    /// <summary>
    /// A class representing an implementation of <see cref="IExampleProvider"/>
    /// for the <see cref="HashResponse"/> class. This class cannot be inherited.
    /// </summary>
    public sealed class HashResponseExampleProvider : IExampleProvider
    {
        /// <inheritdoc />
        public object GetExample()
        {
            return new HashResponse()
            {
                Hash = "NFVO5w7Axj+MxTTlzt9ACDDAJj9ZrMf/GdD7AFIu5i8=",
            };
        }
    }
}
