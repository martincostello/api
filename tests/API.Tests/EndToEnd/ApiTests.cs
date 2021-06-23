﻿// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Shouldly;
using Xunit;

namespace MartinCostello.Api.EndToEnd
{
    public class ApiTests : EndToEndTest
    {
        public ApiTests(ApiFixture fixture)
            : base(fixture)
        {
        }

        [SkippableFact]
        public async Task Can_Get_Time()
        {
            // Arrange
            var tolerance = TimeSpan.FromSeconds(5);
            var utcNow = DateTimeOffset.UtcNow;
            using var client = Fixture.CreateClient();

            // Act
            using var response = await client.GetFromJsonAsync<JsonDocument>("/time");

            // Assert
            response.ShouldNotBeNull();
            response.RootElement.GetProperty("timestamp").GetDateTimeOffset().ShouldBe(utcNow, tolerance);

            DateTimeOffset.TryParse(response.RootElement.GetProperty("rfc1123").GetString(), out DateTimeOffset actual).ShouldBeTrue();
            actual.ShouldBe(utcNow, TimeSpan.FromSeconds(5), "rfc1123 is not a valid DateTimeOffset.");

            DateTimeOffset.TryParse(response.RootElement.GetProperty("universalSortable").GetString(), out actual).ShouldBeTrue();
            actual.ShouldBe(utcNow, TimeSpan.FromSeconds(5), "universalSortable is not a valid DateTimeOffset.");

            DateTimeOffset.TryParse(response.RootElement.GetProperty("universalFull").GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out actual).ShouldBeTrue();
            actual.ShouldBe(utcNow, TimeSpan.FromSeconds(5), "universalFull is not a valid DateTimeOffset.");

            long unix = response.RootElement.GetProperty("unix").GetInt64();
            unix.ShouldBeGreaterThan(DateTimeOffset.UnixEpoch.ToUnixTimeSeconds());
            DateTimeOffset.FromUnixTimeSeconds(unix).ShouldBe(utcNow, TimeSpan.FromSeconds(5), "The value of unix is incorrect.");
        }

        [SkippableFact]
        public async Task Can_Generate_Guid()
        {
            // Arrange
            using var client = Fixture.CreateClient();

            // Act
            using var response = await client.GetFromJsonAsync<JsonDocument>("/tools/guid");

            // Assert
            response.ShouldNotBeNull();
            response.RootElement.GetProperty("guid").GetGuid().ShouldNotBe(Guid.Empty);
        }

        [SkippableFact]
        public async Task Can_Generate_Machine_Key()
        {
            // Arrange
            var parameters = new Dictionary<string, string?>()
            {
                ["decryptionAlgorithm"] = "AES-256",
                ["validationAlgorithm"] = "SHA1",
            };

            using var client = Fixture.CreateClient();

            string requestUri = QueryHelpers.AddQueryString("/tools/machinekey", parameters);

            // Act
            using var response = await client.GetFromJsonAsync<JsonDocument>(requestUri);

            // Assert
            response.ShouldNotBeNull();
            response.RootElement.GetProperty("decryptionKey").GetString().ShouldNotBeNullOrWhiteSpace();
            response.RootElement.GetProperty("validationKey").GetString().ShouldNotBeNullOrWhiteSpace();
            response.RootElement.GetProperty("machineKeyXml").GetString().ShouldNotBeNullOrWhiteSpace();
        }

        [SkippableTheory]
        [InlineData("md5", "hexadecimal", "martincostello.com", "e6c3105bdb8e6466f9db1dab47a85131")]
        [InlineData("sha1", "hexadecimal", "martincostello.com", "7fbd8e8cf806e5282af895396f5268483bf6af1b")]
        [InlineData("sha256", "hexadecimal", "martincostello.com", "3b8143aa8119eaf0910aef5cade45dd0e6bb7b70e8d1c8c057bf3fc125248642")]
        [InlineData("sha384", "hexadecimal", "martincostello.com", "5c0e892a9348c184df255f46ab7282eb5792d552c896eb6893d90f36c7202540a9942c80ce5812616d29c08331c60510")]
        [InlineData("sha512", "hexadecimal", "martincostello.com", "3be0167275455dcf1e34f8818d48b7ae4a61fb8549153f42d0d035464fdccee97022d663549eb249d4796956e4016ad83d5e64ba766fb751c8fb2c03b2b4eb9a")]
        public async Task Can_Generate_Hash(string algorithm, string format, string plaintext, string expected)
        {
            // Arrange
            var request = new
            {
                algorithm,
                format,
                plaintext,
            };

            using var client = Fixture.CreateClient();

            // Act
            using var response = await client.PostAsJsonAsync("/tools/hash", request);

            // Assert
            response.EnsureSuccessStatusCode();

            using var result = JsonDocument.Parse(await response.Content.ReadAsStreamAsync());

            result.RootElement.GetProperty("hash").GetString().ShouldBe(expected);
        }
    }
}
