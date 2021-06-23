// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using MartinCostello.Api.Models;

namespace MartinCostello.Api.Swagger
{
    /// <summary>
    /// A class representing an implementation of <see cref="IExampleProvider"/>
    /// for the <see cref="MachineKeyResponse"/> class. This class cannot be inherited.
    /// </summary>
    public sealed class MachineKeyResponseExampleProvider : IExampleProvider
    {
        /// <inheritdoc />
        public object GetExample()
        {
            return new MachineKeyResponse()
            {
                DecryptionKey = "2EA72C07DEEF522B4686C39BDF83E70A96BA92EE1D960029821FCA2E4CD9FB72",
                ValidationKey = "0A7A92827A74B9B4D2A21918814D8E4A9150BB5ADDB284533BDB50E44ADA6A4BCCFF637A5CB692816EE304121A1BCAA5A6D96BE31A213DEE0BAAEF102A391E8F",
                MachineKeyXml = @"<machineKey validationKey=""0A7A92827A74B9B4D2A21918814D8E4A9150BB5ADDB284533BDB50E44ADA6A4BCCFF637A5CB692816EE304121A1BCAA5A6D96BE31A213DEE0BAAEF102A391E8F"" decryptionKey=""2EA72C07DEEF522B4686C39BDF83E70A96BA92EE1D960029821FCA2E4CD9FB72"" validation=""SHA1"" decryption=""AES"" /> ",
            };
        }
    }
}
