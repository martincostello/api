// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api.Controllers
{
    using System;
    using System.Buffers;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using Swagger;
    using Swashbuckle.AspNetCore.Annotations;

    /// <summary>
    /// A class representing the controller for the <c>/tools</c> resource.
    /// </summary>
    [ApiController]
    [EnableCors(Startup.DefaultCorsPolicyName)]
    [Produces("application/json")]
    [Route("tools")]
    public class ToolsController : ControllerBase
    {
        /// <summary>
        /// An <see cref="IDictionary{K, V}"/> containing the sizes of the decryption and validation hashes for machine keys.
        /// </summary>
        private static readonly IDictionary<string, int> HashSizes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "3DES-D", 24 },
            { "3DES-V", 24 },
            { "AES-128-D", 16 },
            { "AES-192-D", 24 },
            { "AES-256-D", 32 },
            { "AES-V", 32 },
            { "DES-D", 32 },
            { "MD5-V", 16 },
            { "HMACSHA256-V", 32 },
            { "HMACSHA384-V", 48 },
            { "HMACSHA512-V", 64 },
            { "SHA1-V", 64 },
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
        [Produces("application/json", Type = typeof(GuidResponse))]
        [ProducesResponseType(typeof(GuidResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [Route("guid")]
        [SwaggerResponse(StatusCodes.Status200OK, description: "A GUID was generated successfully.", Type = typeof(GuidResponse))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, description: "The specified format is invalid.", Type = typeof(ErrorResponse))]
        [SwaggerResponseExample(typeof(GuidResponse), typeof(GuidResponseExampleProvider))]
        public ActionResult<GuidResponse> Guid([FromQuery]string format = null, [FromQuery]bool? uppercase = null)
        {
            string guid;

            try
            {
                guid = System.Guid.NewGuid().ToString(format ?? "D");
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
        [Consumes("application/json", "text/json")]
        [HttpPost]
        [Produces("application/json", Type = typeof(HashResponse))]
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

            byte[] hash;

            using (var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream, Encoding.ASCII, MaxPlaintextLength, true))
                {
                    writer.Write(request.Plaintext ?? string.Empty);
                    writer.Flush();
                }

                stream.Seek(0, SeekOrigin.Begin);

                using (var hasher = CreateHashAlgorithm(request.Algorithm))
                {
                    if (hasher == null)
                    {
                        return BadRequest($"The specified hash algorithm '{request.Algorithm}' is not supported.");
                    }

                    hash = hasher.ComputeHash(stream);
                }
            }

            return new HashResponse()
            {
                Hash = formatAsBase64 ? Convert.ToBase64String(hash) : BytesToHexString(hash),
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
        [Produces("application/json", Type = typeof(MachineKeyResponse))]
        [ProducesResponseType(typeof(MachineKeyResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [Route("machinekey")]
        [SwaggerResponse(StatusCodes.Status200OK, description: "The machine key was generated successfully.", Type = typeof(MachineKeyResponse))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, description: "The specified decryption or validation algorithm is invalid.", Type = typeof(ErrorResponse))]
        [SwaggerResponseExample(typeof(MachineKeyResponse), typeof(MachineKeyResponseExampleProvider))]
        public ActionResult<MachineKeyResponse> MachineKey([FromQuery]string decryptionAlgorithm, [FromQuery]string validationAlgorithm)
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

            var pool = ArrayPool<byte>.Shared;
            var decryptionKeyBytes = pool.Rent(decryptionKeyLength);
            var validationKeyBytes = pool.Rent(validationKeyLength);

            try
            {
                var decryptionKey = decryptionKeyBytes.AsSpan(0, decryptionKeyLength);
                var validationKey = decryptionKeyBytes.AsSpan(0, validationKeyLength);

                var value = new MachineKeyResponse();

                using (RandomNumberGenerator random = RandomNumberGenerator.Create())
                {
                    random.GetBytes(decryptionKey);
                }

                using (RandomNumberGenerator random = RandomNumberGenerator.Create())
                {
                    random.GetBytes(validationKey);
                }

                value.DecryptionKey = BytesToHexString(decryptionKey).ToUpperInvariant();
                value.ValidationKey = BytesToHexString(validationKey).ToUpperInvariant();

                value.MachineKeyXml = string.Format(
                    CultureInfo.InvariantCulture,
                    @"<machineKey validationKey=""{0}"" decryptionKey=""{1}"" validation=""{2}"" decryption=""{3}"" />",
                    value.ValidationKey,
                    value.DecryptionKey,
                    validationAlgorithm,
                    decryptionAlgorithm);

                return value;
            }
            finally
            {
                pool.Return(decryptionKeyBytes, true);
                pool.Return(validationKeyBytes, true);
            }
        }

        /// <summary>
        /// Returns a <see cref="string"/> containing a hexadecimal representation of the specified <see cref="Array"/> of bytes.
        /// </summary>
        /// <param name="buffer">The buffer to generate the hash string for.</param>
        /// <returns>
        /// A <see cref="string"/> containing the hexadecimal representation of <paramref name="buffer"/>.
        /// </returns>
        private static string BytesToHexString(ReadOnlySpan<byte> buffer)
        {
            var format = new StringBuilder(buffer.Length);

            foreach (var b in buffer)
            {
                format.Append(b.ToString("x2"));
            }

            return format.ToString();
        }

        /// <summary>
        /// Creates a hash algorithm for the specified algorithm name.
        /// </summary>
        /// <param name="name">The name of the hash algorithm to create.</param>
        /// <returns>
        /// The created instance of <see cref="HashAlgorithm"/> if <paramref name="name"/>
        /// is valid; otherwise <see langword="null"/>.
        /// </returns>
        private static HashAlgorithm CreateHashAlgorithm(string name)
        {
            if (string.Equals(name, HashAlgorithmName.MD5.Name, StringComparison.OrdinalIgnoreCase))
            {
                return MD5.Create();
            }
            else if (string.Equals(name, HashAlgorithmName.SHA1.Name, StringComparison.OrdinalIgnoreCase))
            {
                return SHA1.Create();
            }
            else if (string.Equals(name, HashAlgorithmName.SHA256.Name, StringComparison.OrdinalIgnoreCase))
            {
                return SHA256.Create();
            }
            else if (string.Equals(name, HashAlgorithmName.SHA384.Name, StringComparison.OrdinalIgnoreCase))
            {
                return SHA384.Create();
            }
            else if (string.Equals(name, HashAlgorithmName.SHA512.Name, StringComparison.OrdinalIgnoreCase))
            {
                return SHA512.Create();
            }
            else
            {
                return null;
            }
        }

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
                RequestId = HttpContext.TraceIdentifier,
                StatusCode = StatusCodes.Status400BadRequest,
            };

            return new BadRequestObjectResult(error);
        }
    }
}
