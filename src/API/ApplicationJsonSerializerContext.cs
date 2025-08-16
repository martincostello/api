// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using MartinCostello.Api.Models;

namespace MartinCostello.Api;

[ExcludeFromCodeCoverage]
[JsonSerializable(typeof(bool?))]
[JsonSerializable(typeof(GitHubAccessToken))]
[JsonSerializable(typeof(GitHubDeviceCode))]
[JsonSerializable(typeof(GuidResponse))]
[JsonSerializable(typeof(HashRequest))]
[JsonSerializable(typeof(HashResponse))]
[JsonSerializable(typeof(HttpValidationProblemDetails))]
[JsonSerializable(typeof(JsonObject))]
[JsonSerializable(typeof(MachineKeyResponse))]
[JsonSerializable(typeof(TimeResponse))]
[JsonSourceGenerationOptions(
    NumberHandling = JsonNumberHandling.Strict,
    PropertyNameCaseInsensitive = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true)]
public sealed partial class ApplicationJsonSerializerContext : JsonSerializerContext;
