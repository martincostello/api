// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace MartinCostello.Api.Models
{
    /// <summary>
    /// Represents the response from the <c>/tools/guid</c> API resource.
    /// </summary>
    public sealed class GuidResponse
    {
        /// <summary>
        /// Gets or sets the generated GUID value.
        /// </summary>
        [JsonPropertyName("guid")]
#pragma warning disable CA1720
        public string Guid { get; set; } = string.Empty;
#pragma warning restore CA1720
    }
}
