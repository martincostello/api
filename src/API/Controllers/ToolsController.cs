// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using MartinCostello.Api.Models;
using MartinCostello.Api.Swagger;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MartinCostello.Api.Controllers
{
    /// <summary>
    /// A class representing the controller for the <c>/tools</c> resource.
    /// </summary>
    [ApiController]
    [EnableCors(Startup.DefaultCorsPolicyName)]
    [Produces(MediaTypeNames.Application.Json)]
    [Route("tools")]
    public class ToolsController : ControllerBase
    {
        /// <summary>
        /// An <see cref="IDictionary{K, V}"/> containing the sizes of the decryption and validation hashes for machine keys.
        /// </summary>
        private static readonly IDictionary<string, int> HashSizes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
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
        /// Generates a GUID.
        /// </summary>
        /// <param name="format">The format for which to generate a GUID.</param>
        /// <param name="uppercase">Whether to return the GUID in uppercase.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the generated GUID.
        /// </returns>
        [HttpGet]
        [Produces(MediaTypeNames.Application.Json, Type = typeof(GuidResponse))]
        [ProducesResponseType(typeof(GuidResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [Route("guid")]
        [SwaggerResponse(StatusCodes.Status200OK, description: "A GUID was generated successfully.", Type = typeof(GuidResponse))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, description: "The specified format is invalid.", Type = typeof(ErrorResponse))]
        [SwaggerResponseExample(typeof(GuidResponse), typeof(GuidResponseExampleProvider))]
        public ActionResult<GuidResponse> GenerateGuid([FromQuery]string? format = null, [FromQuery]bool? uppercase = null)
        {
            string guid;

            try
            {
                guid = Guid.NewGuid().ToString(format ?? "D", CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                return BadRequest($"The specified format '{format}' is invalid.");
            }

            if (uppercase == true)
            {
                guid = guid.ToUpperInvariant();
            }

            return new GuidResponse()
            {
                Guid = guid,
            };
        }

        /// <summary>
        /// Generates a hash of some plaintext for a specified hash algorithm and returns it in the required format.
        /// </summary>
        /// <param name="request">The hash request.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the generated hash value.
        /// </returns>
        [Consumes(MediaTypeNames.Application.Json, "text/json")]
        [HttpPost]
        [Produces(MediaTypeNames.Application.Json, Type = typeof(HashResponse))]
        [ProducesResponseType(typeof(HashResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [Route("hash")]
        [SwaggerResponse(StatusCodes.Status200OK, description: "The hash was generated successfully.", Type = typeof(HashResponse))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, description: "The specified hash algorithm or output format is invalid.", Type = typeof(ErrorResponse))]
        [SwaggerResponseExample(typeof(HashResponse), typeof(HashResponseExampleProvider))]
        public ActionResult<HashResponse> Hash([FromBody]HashRequest request)
        {
            if (request == null)
            {
                return BadRequest("No hash request specified.");
            }

            if (string.IsNullOrWhiteSpace(request.Algorithm))
            {
                return BadRequest("No hash algorithm name specified.");
            }

            if (string.IsNullOrWhiteSpace(request.Format))
            {
                return BadRequest("No hash output format specified.");
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
                    return BadRequest($"The specified hash format '{request.Format}' is invalid.");
            }

            const int MaxPlaintextLength = 4096;

            if (request.Plaintext?.Length > MaxPlaintextLength)
            {
                return BadRequest($"The plaintext to hash cannot be more than {MaxPlaintextLength} characters in length.");
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
                return BadRequest($"The specified hash algorithm '{request.Algorithm}' is not supported.");
            }

            return new HashResponse()
            {
                Hash = formatAsBase64 ? Convert.ToBase64String(hash) : BytesToHexString(hash).ToLowerInvariant(),
            };
        }

        /// <summary>
        /// Generates a machine key for a <c>Web.config</c> configuration file for ASP.NET.
        /// </summary>
        /// <param name="decryptionAlgorithm">The name of the decryption algorithm.</param>
        /// <param name="validationAlgorithm">The name of the validation algorithm.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the generated machine key.
        /// </returns>
        [HttpGet]
        [Produces(MediaTypeNames.Application.Json, Type = typeof(MachineKeyResponse))]
        [ProducesResponseType(typeof(MachineKeyResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [Route("machinekey")]
        [SwaggerResponse(StatusCodes.Status200OK, description: "The machine key was generated successfully.", Type = typeof(MachineKeyResponse))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, description: "The specified decryption or validation algorithm is invalid.", Type = typeof(ErrorResponse))]
        [SwaggerResponseExample(typeof(MachineKeyResponse), typeof(MachineKeyResponseExampleProvider))]
        public ActionResult<MachineKeyResponse> MachineKey([FromQuery]string? decryptionAlgorithm, [FromQuery]string? validationAlgorithm)
        {
            if (string.IsNullOrEmpty(decryptionAlgorithm) ||
                !HashSizes.TryGetValue(decryptionAlgorithm + "-D", out int decryptionKeyLength))
            {
                return BadRequest($"The specified decryption algorithm '{decryptionAlgorithm}' is invalid.");
            }

            if (string.IsNullOrEmpty(validationAlgorithm) ||
                !HashSizes.TryGetValue(validationAlgorithm + "-V", out int validationKeyLength))
            {
                return BadRequest($"The specified validation algorithm '{validationAlgorithm}' is invalid.");
            }

            var decryptionKey = new byte[decryptionKeyLength];
            var validationKey = new byte[validationKeyLength];

            RandomNumberGenerator.Fill(decryptionKey);
            RandomNumberGenerator.Fill(validationKey);

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

            return result;
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

        /// <summary>
        /// Returns a result that represents a bad API request.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <returns>
        /// An <see cref="BadRequestObjectResult"/> that represents an invalid API request.
        /// </returns>
        private BadRequestObjectResult BadRequest(string message)
        {
            var error = new ErrorResponse()
            {
                Message = message,
                RequestId = HttpContext?.TraceIdentifier ?? string.Empty,
                StatusCode = StatusCodes.Status400BadRequest,
            };

            return new BadRequestObjectResult(error);
        }
    }
}
