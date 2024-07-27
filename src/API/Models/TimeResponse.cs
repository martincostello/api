// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;
using MartinCostello.Api.OpenApi;

namespace MartinCostello.Api.Models;

/// <summary>
/// Represents the response from the <c>/time</c> API resource.
/// </summary>
[OpenApiExample<TimeResponse>]
public sealed class TimeResponse : IExampleProvider<TimeResponse>
{
    /// <summary>
    /// Gets or sets the timestamp for the response for which the times are generated.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the current UTC date and time in RFC1123 format.
    /// </summary>
    [JsonPropertyName("rfc1123")]
    public string Rfc1123 { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of seconds since the UNIX epoch.
    /// </summary>
    [JsonPropertyName("unix")]
    public long Unix { get; set; }

    /// <summary>
    /// Gets or sets the current UTC date and time in universal sortable format.
    /// </summary>
    [JsonPropertyName("universalSortable")]
    public string UniversalSortable { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current UTC date and time in universal full format.
    /// </summary>
    [JsonPropertyName("universalFull")]
    public string UniversalFull { get; set; } = string.Empty;

    /// <inheritdoc/>
    public static TimeResponse GenerateExample()
    {
        return new()
        {
            Timestamp = new(2016, 6, 3, 18, 44, 14, TimeSpan.Zero),
            Rfc1123 = "Fri, 03 Jun 2016 18:44:14 GMT",
            UniversalFull = "Friday, 03 June 2016 18:44:14",
            UniversalSortable = "2016-06-03 18:44:14Z",
            Unix = 1464979454,
        };
    }
}
