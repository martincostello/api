// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;
using MartinCostello.Api.Swagger;

namespace MartinCostello.Api.Models
{
    /// <summary>
    /// Represents an error response from an API resource.
    /// </summary>
    [SwaggerTypeExample(typeof(ErrorResponseExampleProvider))]
    public sealed class ErrorResponse
    {
        /// <summary>
        /// Gets or sets the HTTP status code.
        /// </summary>
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the request Id.
        /// </summary>
        [JsonPropertyName("requestId")]
        public string RequestId { get; set; } = string.Empty;
    }
}
