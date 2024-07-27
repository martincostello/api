// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;
using MartinCostello.Api.OpenApi;

namespace MartinCostello.Api.Models;

/// <summary>
/// Represents the response from the <c>/tools/hash</c> API resource.
/// </summary>
[OpenApiExample<HashResponse>]
public sealed class HashResponse : IExampleProvider<HashResponse>
{
    /// <summary>
    /// Gets or sets a string containing the generated hash value in the requested format.
    /// </summary>
    [JsonPropertyName("hash")]
    public string Hash { get; set; } = string.Empty;

    /// <inheritdoc/>
    public static HashResponse GenerateExample()
    {
        return new()
        {
            Hash = "fTi1zSWiuvha07tbkxE4PmcaihQuswKzJNSl+6h0jGk=",
        };
    }
}
