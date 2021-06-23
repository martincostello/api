// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace MartinCostello.Api.Models
{
    /// <summary>
    /// Represents the response from the <c>/tools/machinekey</c> API resource.
    /// </summary>
    public sealed class MachineKeyResponse
    {
        /// <summary>
        /// Gets or sets a string containing the decryption key.
        /// </summary>
        [JsonPropertyName("decryptionKey")]
        public string DecryptionKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a string containing the validation key.
        /// </summary>
        [JsonPropertyName("validationKey")]
        public string ValidationKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a string containing the <c>&lt;machineKey&gt;</c> XML configuration element.
        /// </summary>
        [JsonPropertyName("machineKeyXml")]
        public string MachineKeyXml { get; set; } = string.Empty;
    }
}
