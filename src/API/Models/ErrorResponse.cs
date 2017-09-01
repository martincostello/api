// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api.Models
{
    using MartinCostello.Api.Swagger;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents an error response from an API resource.
    /// </summary>
    [SwaggerTypeExample(typeof(ErrorResponseExampleProvider))]
    public sealed class ErrorResponse
    {
        /// <summary>
        /// Gets or sets the HTTP status code.
        /// </summary>
        [JsonProperty("statusCode")]
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the request Id.
        /// </summary>
        [JsonProperty("requestId")]
        public string RequestId { get; set; }
    }
}
