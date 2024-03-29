﻿// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;
using MartinCostello.Api.OpenApi;
using Newtonsoft.Json;

namespace MartinCostello.Api.Models;

/// <summary>
/// Represents the response from the <c>/tools/guid</c> API resource.
/// </summary>
public sealed class GuidResponse : IExampleProvider<GuidResponse>
{
    /// <summary>
    /// Gets or sets the generated GUID value.
    /// </summary>
    [JsonProperty("guid")]
    [JsonPropertyName("guid")]
#pragma warning disable CA1720
    public string Guid { get; set; } = string.Empty;
#pragma warning restore CA1720

    /// <inheritdoc/>
    static object IExampleProvider<GuidResponse>.GenerateExample()
    {
        return new GuidResponse()
        {
            Guid = new("6bc55a07-3d3e-4d52-8701-362a1187772d"),
        };
    }
}
