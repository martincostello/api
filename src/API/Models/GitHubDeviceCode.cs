// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace MartinCostello.Api.Models;

/// <summary>
/// A class representing a GitHub device code. This class cannot be inherited.
/// </summary>
public sealed class GitHubDeviceCode
{
    /// <summary>
    /// Gets or sets the device code.
    /// </summary>
    [JsonPropertyName("device_code")]
    public string DeviceCode { get; set; } = default!;

    /// <summary>
    /// Gets or sets the user code.
    /// </summary>
    [JsonPropertyName("user_code")]
    public string UserCode { get; set; } = default!;

    /// <summary>
    /// Gets or sets the verification URL.
    /// </summary>
    [JsonPropertyName("verification_uri")]
    public string VerificationUrl { get; set; } = default!;

    /// <summary>
    /// Gets or sets the number of seconds of seconds before the device and user codes expire.
    /// </summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresInSeconds { get; set; }

    /// <summary>
    /// Gets or sets the minimum number of seconds that must pass before re-requesting an access token.
    /// </summary>
    [JsonPropertyName("interval")]
    public int RefreshIntervalInSeconds { get; set; }
}
