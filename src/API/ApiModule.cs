// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using MartinCostello.Api.Models;
using MartinCostello.OpenApi;
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
        var group = builder.MapGroup(string.Empty)
                           .RequireCors("DefaultCorsPolicy")
                           .WithTags("API");

        group.MapGet("/time", GetTime)
             .WithName("Time")
             .WithSummary("Gets the current UTC time.")
             .WithDescription("Gets the current date and time in UTC.")
             .ProducesOpenApiResponse(StatusCodes.Status200OK, "The current UTC date and time.");

        group.MapGet("/tools/guid", GenerateGuid)
             .WithName("Guid")
             .WithSummary("Generates a GUID.")
             .WithDescription("Generates a new GUID in the specified format.")
             .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")
             .ProducesOpenApiResponse(StatusCodes.Status200OK, "A GUID was generated successfully.")
             .ProducesOpenApiResponse(StatusCodes.Status400BadRequest, "The specified format is invalid.");

        group.MapPost("/tools/hash", GenerateHash)
             .WithName("Hash")
             .WithSummary("Hashes a string.")
             .WithDescription("Generates a hash of some plaintext for a specified hash algorithm and returns it in the required format.")
             .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")
             .ProducesOpenApiResponse(StatusCodes.Status200OK, "The hash was generated successfully.")
             .ProducesOpenApiResponse(StatusCodes.Status400BadRequest, "The specified hash algorithm or output format is invalid.");

        group.MapGet("/tools/machinekey", GenerateMachineKey)
             .WithName("MachineKey")
             .WithSummary("Generates a machine key.")
             .WithDescription("Generates a machine key for a Web.config configuration file for ASP.NET.")
             .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")
             .ProducesOpenApiResponse(StatusCodes.Status200OK, "The machine key was generated successfully.")
             .ProducesOpenApiResponse(StatusCodes.Status400BadRequest, "The specified decryption or validation algorithm is invalid.");

        builder.MapGet("/version", static () =>
                {
                    return new JsonObject()
                    {
                        ["applicationVersion"] = GitMetadata.Version,
                        ["frameworkDescription"] = RuntimeInformation.FrameworkDescription,
                        ["operatingSystem"] = new JsonObject()
                        {
                            ["description"] = RuntimeInformation.OSDescription,
                            ["architecture"] = RuntimeInformation.OSArchitecture.ToString(),
                            ["version"] = Environment.OSVersion.VersionString,
                            ["is64Bit"] = Environment.Is64BitOperatingSystem,
                        },
                        ["process"] = new JsonObject()
                        {
                            ["architecture"] = RuntimeInformation.ProcessArchitecture.ToString(),
                            ["is64BitProcess"] = Environment.Is64BitProcess,
                            ["isNativeAoT"] = !RuntimeFeature.IsDynamicCodeSupported,
                            ["isPrivilegedProcess"] = Environment.IsPrivilegedProcess,
                        },
                        ["dotnetVersions"] = new JsonObject()
                        {
                            ["runtime"] = GetVersion<object>(),
                            ["aspNetCore"] = GetVersion<HttpContext>(),
                        },
                    };

                    static string GetVersion<T>()
                        => typeof(T).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
                })
               .WithName("Version")
               .ExcludeFromDescription();

        return builder;
    }

    /// <summary>
    /// Returns a <see cref="string"/> containing a hexadecimal representation of the specified <see cref="ReadOnlySpan{T}"/> of bytes.
    /// </summary>
    /// <param name="bytes">The buffer to generate the hash string for.</param>
    /// <param name="toLower">Whether to return the hash in lowercase.</param>
    /// <returns>
    /// A <see cref="string"/> containing the hexadecimal representation of <paramref name="bytes"/>.
    /// </returns>
    private static string BytesToHexString(ReadOnlySpan<byte> bytes, bool toLower = false)
        => toLower ? Convert.ToHexStringLower(bytes) : Convert.ToHexString(bytes);

    private static Ok<TimeResponse> GetTime(TimeProvider timeProvider)
        => TypedResults.Ok(TimeService.Now(timeProvider));

    private static Results<Ok<GuidResponse>, ValidationProblem> GenerateGuid(
        [Description("The format for which to generate a GUID.")][OpenApiExample("D")] string? format,
        [Description("Whether to return the GUID in uppercase.")] bool? uppercase)
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

        if (uppercase is true)
        {
            guid = guid.ToUpperInvariant();
        }

        return TypedResults.Ok(new GuidResponse() { Guid = guid });
    }

    private static Results<Ok<HashResponse>, ValidationProblem> GenerateHash(HashRequest? request)
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
        HashAlgorithmName? hashAlgorithm = request.Algorithm.ToUpperInvariant() switch
        {
            "MD5" => HashAlgorithmName.MD5,
            "SHA1" => HashAlgorithmName.SHA1,
            "SHA256" => HashAlgorithmName.SHA256,
            "SHA384" => HashAlgorithmName.SHA384,
            "SHA512" => HashAlgorithmName.SHA512,
            _ => null,
        };

        if (hashAlgorithm is not { } algorithm)
        {
            return Results.Extensions.InvalidRequest($"The specified hash algorithm '{request.Algorithm}' is not supported.");
        }

        byte[] hash = CryptographicOperations.HashData(algorithm, buffer);

        var result = new HashResponse()
        {
            Hash = formatAsBase64 ? Convert.ToBase64String(hash) : BytesToHexString(hash, toLower: true),
        };

        return TypedResults.Ok(result);
    }

    private static Results<Ok<MachineKeyResponse>, ValidationProblem> GenerateMachineKey(
        [Description("The name of the decryption algorithm.")][OpenApiExample("AES-256")] string? decryptionAlgorithm,
        [Description("The name of the validation algorithm.")][OpenApiExample("SHA1")] string? validationAlgorithm)
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

        return TypedResults.Ok(result);
    }
}
