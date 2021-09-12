// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text;
using MartinCostello.Api.Models;
using MartinCostello.Api.Swagger;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Swashbuckle.AspNetCore.Annotations;

namespace MartinCostello.Api;

/// <summary>
/// A class that configures the API endpoints.
/// </summary>
public static class ApiModule
{
    /// <summary>
    /// An dictionary containing the sizes of the decryption and validation hashes for machine keys.
    /// </summary>
    private static readonly Dictionary<string, int> HashSizes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["3DES-D"] = 24,
        ["3DES-V"] = 24,
        ["AES-128-D"] = 16,
        ["AES-192-D"] = 24,
        ["AES-256-D"] = 32,
        ["AES-V"] = 32,
        ["DES-D"] = 32,
        ["MD5-V"] = 16,
        ["HMACSHA256-V"] = 32,
        ["HMACSHA384-V"] = 48,
        ["HMACSHA512-V"] = 64,
        ["SHA1-V"] = 64,
    };

    /// <summary>
    /// Maps the API endpoints.
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointRouteBuilder"/> to use.</param>
    /// <returns>
    /// The <see cref="IEndpointRouteBuilder"/> specified by <paramref name="builder"/>.
    /// </returns>
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/time", (IClock clock) =>
        {
            var formatProvider = CultureInfo.InvariantCulture;
            var now = clock.GetCurrentInstant().ToDateTimeOffset();

            return new TimeResponse()
            {
                Timestamp = now,
                Rfc1123 = now.ToString("r", formatProvider),
                UniversalFull = now.UtcDateTime.ToString("U", formatProvider),
                UniversalSortable = now.UtcDateTime.ToString("u", formatProvider),
                Unix = now.ToUnixTimeSeconds(),
            };
        })
        .Produces<TimeResponse>("The current UTC date and time.")
        .RequireCors("DefaultCorsPolicy")
        .WithOperationDescription("Gets the current UTC time.")
        .WithResponseExample<TimeResponse, TimeResponseExampleProvider>()
        .WithName("Time");

        builder.MapGet("/tools/guid", (
            [SwaggerParameter("The format for which to generate a GUID.")] string? format,
            [SwaggerParameter("Whether to return the GUID in uppercase.")] bool? uppercase) =>
        {
            string guid;

            try
            {
                guid = Guid.NewGuid().ToString(format ?? "D", CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                return Results.Problem($"The specified format '{format}' is invalid.", statusCode: StatusCodes.Status400BadRequest);
            }

            if (uppercase == true)
            {
                guid = guid.ToUpperInvariant();
            }

            return Results.Json(new GuidResponse() { Guid = guid });
        })
        .Produces<GuidResponse>("A GUID was generated successfully.")
        .ProducesProblem("The specified format is invalid.")
        .WithOperationDescription("Generates a GUID.")
        .WithResponseExample<GuidResponse, GuidResponseExampleProvider>()
        .WithResponseExample<ProblemDetails, ProblemDetailsExampleProvider>()
        .WithName("Guid");

        builder.MapPost("/tools/hash", (HashRequest? request) =>
        {
            if (request == null)
            {
                return Results.Problem("No hash request specified.", statusCode: StatusCodes.Status400BadRequest);
            }

            if (string.IsNullOrWhiteSpace(request.Algorithm))
            {
                return Results.Problem("No hash algorithm name specified.", statusCode: StatusCodes.Status400BadRequest);
            }

            if (string.IsNullOrWhiteSpace(request.Format))
            {
                return Results.Problem("No hash output format specified.", statusCode: StatusCodes.Status400BadRequest);
            }

            bool formatAsBase64;

            switch (request.Format.ToUpperInvariant())
            {
                case "BASE64":
                    formatAsBase64 = true;
                    break;

                case "HEXADECIMAL":
                    formatAsBase64 = false;
                    break;

                default:
                    return Results.Problem($"The specified hash format '{request.Format}' is invalid.", statusCode: StatusCodes.Status400BadRequest);
            }

            const int MaxPlaintextLength = 4096;

            if (request.Plaintext?.Length > MaxPlaintextLength)
            {
                return Results.Problem($"The plaintext to hash cannot be more than {MaxPlaintextLength} characters in length.", statusCode: StatusCodes.Status400BadRequest);
            }

            byte[] buffer = Encoding.UTF8.GetBytes(request.Plaintext ?? string.Empty);
            byte[] hash = request.Algorithm.ToUpperInvariant() switch
            {
#pragma warning disable CA5350
#pragma warning disable CA5351
                "MD5" => MD5.HashData(buffer),
                "SHA1" => SHA1.HashData(buffer),
#pragma warning restore CA5350
#pragma warning restore CA5351
                "SHA256" => SHA256.HashData(buffer),
                "SHA384" => SHA384.HashData(buffer),
                "SHA512" => SHA512.HashData(buffer),
                _ => Array.Empty<byte>(),
            };

            if (hash.Length == 0)
            {
                return Results.Problem($"The specified hash algorithm '{request.Algorithm}' is not supported.", statusCode: StatusCodes.Status400BadRequest);
            }

            var result = new HashResponse()
            {
                Hash = formatAsBase64 ? Convert.ToBase64String(hash) : BytesToHexString(hash).ToLowerInvariant(),
            };

            return Results.Json(result);
        })
        .Accepts<HashRequest>("application/json")
        .Produces<HashResponse>("The hash was generated successfully.")
        .ProducesProblem("The specified hash algorithm or output format is invalid.")
        .WithOperationDescription("Generates a hash of some plaintext for a specified hash algorithm and returns it in the required format.")
        .WithRequestExample<HashRequest, HashRequestExampleProvider>()
        .WithResponseExample<HashResponse, HashResponseExampleProvider>()
        .WithResponseExample<ProblemDetails, ProblemDetailsExampleProvider>()
        .WithName("Hash");

        builder.MapGet("/tools/machinekey", (
            [SwaggerParameter("The name of the decryption algorithm.")] string? decryptionAlgorithm,
            [SwaggerParameter("The name of the validation algorithm.")] string? validationAlgorithm) =>
        {
            if (string.IsNullOrEmpty(decryptionAlgorithm) ||
                !HashSizes.TryGetValue(decryptionAlgorithm + "-D", out int decryptionKeyLength))
            {
                return Results.Problem($"The specified decryption algorithm '{decryptionAlgorithm}' is invalid.", statusCode: StatusCodes.Status400BadRequest);
            }

            if (string.IsNullOrEmpty(validationAlgorithm) ||
                !HashSizes.TryGetValue(validationAlgorithm + "-V", out int validationKeyLength))
            {
                return Results.Problem($"The specified validation algorithm '{validationAlgorithm}' is invalid.", statusCode: StatusCodes.Status400BadRequest);
            }

            byte[] decryptionKey = RandomNumberGenerator.GetBytes(decryptionKeyLength);
            byte[] validationKey = RandomNumberGenerator.GetBytes(validationKeyLength);

            var result = new MachineKeyResponse()
            {
                DecryptionKey = BytesToHexString(decryptionKey),
                ValidationKey = BytesToHexString(validationKey),
            };

            result.MachineKeyXml = string.Format(
                CultureInfo.InvariantCulture,
                @"<machineKey validationKey=""{0}"" decryptionKey=""{1}"" validation=""{2}"" decryption=""{3}"" />",
                result.ValidationKey,
                result.DecryptionKey,
                validationAlgorithm.Split('-', StringSplitOptions.RemoveEmptyEntries)[0].ToUpperInvariant(),
                decryptionAlgorithm.Split('-', StringSplitOptions.RemoveEmptyEntries)[0].ToUpperInvariant());

            return Results.Json(result);
        })
        .Produces<MachineKeyResponse>("The machine key was generated successfully.")
        .ProducesProblem("The specified decryption or validation algorithm is invalid.")
        .WithOperationDescription("Generates a machine key for a Web.config configuration file for ASP.NET.")
        .WithResponseExample<MachineKeyResponse, MachineKeyResponseExampleProvider>()
        .WithResponseExample<ProblemDetails, ProblemDetailsExampleProvider>()
        .WithName("MachineKey");

        return builder;
    }

    /// <summary>
    /// Adds the <see cref="SwaggerOperationAttribute"/> to the metadata for all builders produced by builder.
    /// </summary>
    /// <param name="builder">The <see cref="DelegateEndpointConventionBuilder"/>.</param>
    /// <param name="summary">The operation summary.</param>
    /// <param name="description">The optional operation description.</param>
    /// <returns>
    /// A <see cref="DelegateEndpointConventionBuilder"/> that can be used to further customize the endpoint.
    /// </returns>
    private static DelegateEndpointConventionBuilder WithOperationDescription(
        this DelegateEndpointConventionBuilder builder,
        string summary,
        string? description = null)
    {
        return builder.WithMetadata(new SwaggerOperationAttribute(summary, description));
    }

    /// <summary>
    /// Adds <see cref="SwaggerResponseAttribute"/> to the metadata for all builders produced by builder.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="builder">The <see cref="DelegateEndpointConventionBuilder"/>.</param>
    /// <param name="description">The response description.</param>
    /// <param name="statusCode">The response status code. Defaults to <see cref="StatusCodes.Status200OK"/>.</param>
    /// <param name="contentType">The response content type. Defaults to <c>application/json</c>.</param>
    /// <returns>
    /// A <see cref="DelegateEndpointConventionBuilder"/> that can be used to further customize the endpoint.
    /// </returns>
    private static DelegateEndpointConventionBuilder Produces<TResponse>(
        this DelegateEndpointConventionBuilder builder,
        string description,
        int statusCode = StatusCodes.Status200OK,
        string? contentType = null)
    {
        return builder
            .Produces<TResponse>(statusCode, contentType)
            .WithMetadata(new SwaggerResponseAttribute(statusCode) { Type = typeof(TResponse), Description = description });
    }

    /// <summary>
    /// Adds <see cref="SwaggerResponseAttribute"/> to the metadata for all builders produced by builder.
    /// </summary>
    /// <param name="builder">The <see cref="DelegateEndpointConventionBuilder"/>.</param>
    /// <param name="description">The response description.</param>
    /// <param name="statusCode">The response status code. Defaults to <see cref="StatusCodes.Status400BadRequest"/>.</param>
    /// <returns>
    /// A <see cref="DelegateEndpointConventionBuilder"/> that can be used to further customize the endpoint.
    /// </returns>
    private static DelegateEndpointConventionBuilder ProducesProblem(
        this DelegateEndpointConventionBuilder builder,
        string description,
        int statusCode = StatusCodes.Status400BadRequest)
    {
        return builder.Produces<ProblemDetails>(
            description,
            statusCode,
            "application/problem+json");
    }

    /// <summary>
    /// Adds the <see cref="SwaggerRequestExampleAttribute"/> to the metadata for all builders produced by builder.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TExampleProvider">The type of the example provider.</typeparam>
    /// <param name="builder">The <see cref="DelegateEndpointConventionBuilder"/>.</param>
    /// <returns>
    /// A <see cref="DelegateEndpointConventionBuilder"/> that can be used to further customize the endpoint.
    /// </returns>
    private static DelegateEndpointConventionBuilder WithRequestExample<TRequest, TExampleProvider>(this DelegateEndpointConventionBuilder builder)
        where TExampleProvider : IExampleProvider
    {
        return builder.WithMetadata(new SwaggerRequestExampleAttribute(typeof(TRequest), typeof(TExampleProvider)));
    }

    /// <summary>
    /// Adds the <see cref="SwaggerResponseExampleAttribute"/> to the metadata for all builders produced by builder.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <typeparam name="TExampleProvider">The type of the example provider.</typeparam>
    /// <param name="builder">The <see cref="DelegateEndpointConventionBuilder"/>.</param>
    /// <returns>
    /// A <see cref="DelegateEndpointConventionBuilder"/> that can be used to further customize the endpoint.
    /// </returns>
    private static DelegateEndpointConventionBuilder WithResponseExample<TResponse, TExampleProvider>(this DelegateEndpointConventionBuilder builder)
        where TExampleProvider : IExampleProvider
    {
        return builder.WithMetadata(new SwaggerResponseExampleAttribute(typeof(TResponse), typeof(TExampleProvider)));
    }

    /// <summary>
    /// Returns a <see cref="string"/> containing a hexadecimal representation of the specified <see cref="ReadOnlySpan{T}"/> of bytes.
    /// </summary>
    /// <param name="bytes">The buffer to generate the hash string for.</param>
    /// <returns>
    /// A <see cref="string"/> containing the hexadecimal representation of <paramref name="bytes"/>.
    /// </returns>
    private static string BytesToHexString(ReadOnlySpan<byte> bytes)
        => Convert.ToHexString(bytes);
}
