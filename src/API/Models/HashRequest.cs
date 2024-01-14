﻿// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MartinCostello.Api.OpenApi;
using Newtonsoft.Json;

namespace MartinCostello.Api.Models;

/// <summary>
/// Represents a request to the <c>/tools/hash</c> API resource.
/// </summary>
public sealed class HashRequest : IExampleProvider<HashRequest>
{
    /// <summary>
    /// Gets or sets the name of the hash algorithm to use.
    /// </summary>
    [JsonProperty("algorithm")]
    [JsonPropertyName("algorithm")]
    [Required]
    public string Algorithm { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the format in which to return the hash.
    /// </summary>
    [JsonProperty("format")]
    [JsonPropertyName("format")]
    [Required]
    public string Format { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the plaintext value to generate the hash from.
    /// </summary>
    [JsonProperty("plaintext")]
    [JsonPropertyName("plaintext")]
    [Required]
    public string Plaintext { get; set; } = string.Empty;

    /// <inheritdoc/>
    static object IExampleProvider<HashRequest>.GenerateExample()
    {
        return new HashRequest()
        {
            Algorithm = "sha256",
            Format = "base64",
            Plaintext = "The quick brown fox jumped over the lazy dog",
        };
    }
}
