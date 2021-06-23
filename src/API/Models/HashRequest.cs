// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MartinCostello.Api.Models
{
    /// <summary>
    /// Represents a request to the <c>/tools/hash</c> API resource.
    /// </summary>
    public sealed class HashRequest
    {
        /// <summary>
        /// Gets or sets the name of the hash algorithm to use.
        /// </summary>
        [JsonPropertyName("algorithm")]
        [Required]
        public string Algorithm { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the format in which to return the hash.
        /// </summary>
        [JsonPropertyName("format")]
        [Required]
        public string Format { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the plaintext value to generate the hash from.
        /// </summary>
        [JsonPropertyName("plaintext")]
        [Required]
        public string Plaintext { get; set; } = string.Empty;
    }
}
