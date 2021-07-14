// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Xunit;

namespace MartinCostello.Api.EndToEnd
{
    public sealed class ApiFixture
    {
        private const string WebsiteUrl = "WEBSITE_URL";

        public ApiFixture()
        {
            string url = Environment.GetEnvironmentVariable(WebsiteUrl) ?? string.Empty;

            if (Uri.TryCreate(url, UriKind.Absolute, out Uri? address))
            {
                ServerAddress = address;
            }
        }

        public Uri? ServerAddress { get; }

        public HttpClient CreateClient()
        {
            Skip.If(ServerAddress is null, $"The {WebsiteUrl} environment variable is not set or is not a valid absolute URI.");

            var client = new HttpClient()
            {
                BaseAddress = ServerAddress,
            };

            client.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue(
                    "MartinCostello.Api.Tests",
                    "1.0.0+" + GitMetadata.Commit));

            return client;
        }
    }
}
