// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the response from the <c>/tools/guid</c> API resource.
    /// </summary>
    public sealed class GuidResponse
    {
        /// <summary>
        /// Gets or sets the generated GUID value.
        /// </summary>
        [JsonProperty("guid")]
        public string Guid { get; set; }
    }
}
