// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;

    /// <summary>
    /// A class representing the response from the <c>/api/time</c> API resource. This class cannot be inherited.
    /// </summary>
    public sealed class TimeResponse
    {
        /// <summary>
        /// Gets or sets the timestamp for the response for which the times are generated.
        /// </summary>
        [JsonProperty("timestamp")]
        [Required]
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the current UTC date and time in RFC1123 format.
        /// </summary>
        [JsonProperty("rfc1123")]
        [Required]
        public string Rfc1123 { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds since the UNIX epoch.
        /// </summary>
        [JsonProperty("unix")]
        [Required]
        public long Unix { get; set; }

        /// <summary>
        /// Gets or sets the current UTC date and time in universal sortable format.
        /// </summary>
        [JsonProperty("universalSortable")]
        [Required]
        public string UniversalSortable { get; set; }

        /// <summary>
        /// Gets or sets the current UTC date and time in universal full format.
        /// </summary>
        [JsonProperty("universalFull")]
        [Required]
        public string UniversalFull { get; set; }
    }
}
