// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the response from the <c>/tools/machinekey</c> API resource.
    /// </summary>
    public sealed class MachineKeyResponse
    {
        /// <summary>
        /// Gets or sets a string containing the decryption key.
        /// </summary>
        [JsonProperty("decryptionKey")]
        public string DecryptionKey { get; set; }

        /// <summary>
        /// Gets or sets a string containing the validation key.
        /// </summary>
        [JsonProperty("validationKey")]
        public string ValidationKey { get; set; }

        /// <summary>
        /// Gets or sets a string containing the <c>&lt;machineKey&gt;</c> XML configuration element.
        /// </summary>
        [JsonProperty("machineKeyXml")]
        public string MachineKeyXml { get; set; }
    }
}
