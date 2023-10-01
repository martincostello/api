// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text;
using MartinCostello.Api.Extensions;
using MartinCostello.Api.Models;
using MartinCostello.Api.Swagger;
using Microsoft.AspNetCore.Http.HttpResults;

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
        ["AES-256-D"] = SHA256.HashSizeInBytes,
        ["AES-V"] = 32,
        ["DES-D"] = 32,
        ["MD5-V"] = MD5.HashSizeInBytes,
        ["HMACSHA256-V"] = SHA256.HashSizeInBytes,
        ["HMACSHA384-V"] = SHA384.HashSizeInBytes,
        ["HMACSHA512-V"] = SHA512.HashSizeInBytes,
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
        builder.MapGet("/time", (TimeProvider timeProvider) =>
        {
            var formatProvider = CultureInfo.InvariantCulture;
            var now = timeProvider.GetUtcNow();

            var result = new TimeResponse()
            {
                Timestamp = now,
                Rfc1123 = now.ToString("r", formatProvider),
                UniversalFull = now.UtcDateTime.ToString("U", formatProvider),
                UniversalSortable = now.UtcDateTime.ToString("u", formatProvider),
                Unix = now.ToUnixTimeSeconds(),
            };

            return TypedResults.Ok(result);
        })
        .Produces<TimeResponse, TimeResponseExampleProvider>("The current UTC date and time.")
        .RequireCors("DefaultCorsPolicy")
        .WithName("Time")
        .WithOperationDescription("Gets the current UTC time.");

        builder.MapGet("/tools/guid", Results<JsonHttpResult<GuidResponse>, ProblemHttpResult>
            (
            [SwaggerParameterExample("The format for which to generate a GUID.", "D")] string? format,
            [SwaggerParameterExample("Whether to return the GUID in uppercase.")] bool? uppercase) =>
        {
            string guid;

            try
            {
                guid = Guid.NewGuid().ToString(format ?? "D", CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                return Results.Extensions.InvalidRequest($"The specified format '{format}' is invalid.");
            }

            if (uppercase == true)
            {
                guid = guid.ToUpperInvariant();
            }

            return TypedResults.Json(new GuidResponse() { Guid = guid });
        })
        .Produces<GuidResponse, GuidResponseExampleProvider>("A GUID was generated successfully.")
        .ProducesProblem("The specified format is invalid.")
        .WithName("Guid")
        .WithOperationDescription("Generates a GUID.")
        .WithProblemDetailsResponseExample();

        builder.MapPost("/tools/hash", Results<JsonHttpResult<HashResponse>, ProblemHttpResult>
            (HashRequest? request) =>
        {
            if (request == null)
            {
                return Results.Extensions.InvalidRequest("No hash request specified.");
            }

            if (string.IsNullOrWhiteSpace(request.Algorithm))
            {
                return Results.Extensions.InvalidRequest("No hash algorithm name specified.");
            }

            if (string.IsNullOrWhiteSpace(request.Format))
            {
                return Results.Extensions.InvalidRequest("No hash output format specified.");
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
                    return Results.Extensions.InvalidRequest($"The specified hash format '{request.Format}' is invalid.");
            }

            const int MaxPlaintextLength = 4096;

            if (request.Plaintext?.Length > MaxPlaintextLength)
            {
                return Results.Extensions.InvalidRequest($"The plaintext to hash cannot be more than {MaxPlaintextLength} characters in length.");
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
                _ => [],
            };

            if (hash.Length == 0)
            {
                return Results.Extensions.InvalidRequest($"The specified hash algorithm '{request.Algorithm}' is not supported.");
            }

            var result = new HashResponse()
            {
                Hash = formatAsBase64 ? Convert.ToBase64String(hash) : BytesToHexString(hash).ToLowerInvariant(),
            };

            return TypedResults.Json(result);
        })
        .Accepts<HashRequest, HashRequestExampleProvider>()
        .Produces<HashResponse, HashResponseExampleProvider>("The hash was generated successfully.")
        .ProducesProblem("The specified hash algorithm or output format is invalid.")
        .WithName("Hash")
        .WithOperationDescription("Generates a hash of some plaintext for a specified hash algorithm and returns it in the required format.")
        .WithProblemDetailsResponseExample();

        builder.MapGet("/tools/machinekey", Results<JsonHttpResult<MachineKeyResponse>, ProblemHttpResult>
            (
            [SwaggerParameterExample("The name of the decryption algorithm.", "AES-256")] string? decryptionAlgorithm,
            [SwaggerParameterExample("The name of the validation algorithm.", "SHA1")] string? validationAlgorithm) =>
        {
            if (string.IsNullOrEmpty(decryptionAlgorithm) ||
                !HashSizes.TryGetValue(decryptionAlgorithm + "-D", out int decryptionKeyLength))
            {
                return Results.Extensions.InvalidRequest($"The specified decryption algorithm '{decryptionAlgorithm}' is invalid.");
            }

            if (string.IsNullOrEmpty(validationAlgorithm) ||
                !HashSizes.TryGetValue(validationAlgorithm + "-V", out int validationKeyLength))
            {
                return Results.Extensions.InvalidRequest($"The specified validation algorithm '{validationAlgorithm}' is invalid.");
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

            return TypedResults.Json(result);
        })
        .Produces<MachineKeyResponse, MachineKeyResponseExampleProvider>("The machine key was generated successfully.")
        .ProducesProblem("The specified decryption or validation algorithm is invalid.")
        .WithName("MachineKey")
        .WithOperationDescription("Generates a machine key for a Web.config configuration file for ASP.NET.")
        .WithProblemDetailsResponseExample();

        return builder;
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
