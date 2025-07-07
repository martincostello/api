// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Net.Http.Headers;
using Grpc.Net.Client;

namespace MartinCostello.Api.EndToEnd;

public sealed class ApiFixture
{
    private const string WebsiteUrl = "WEBSITE_URL";

    private static readonly ProductInfoHeaderValue UserAgent = new("MartinCostello.Api.Tests", "1.0.0+" + GitMetadata.Commit);

    public ApiFixture()
    {
        string url = Environment.GetEnvironmentVariable(WebsiteUrl) ?? string.Empty;

        if (Uri.TryCreate(url, UriKind.Absolute, out var address))
        {
            ServerAddress = address;
        }
    }

    public static string UserAgentString => UserAgent.ToString();

    public Uri? ServerAddress { get; }

    public HttpClient CreateClient()
    {
        Assert.SkipWhen(ServerAddress is null, $"The {WebsiteUrl} environment variable is not set or is not a valid absolute URI.");

        var client = new HttpClient()
        {
            BaseAddress = ServerAddress,
        };

        client.DefaultRequestHeaders.UserAgent.Add(UserAgent);

        return client;
    }

    public GrpcChannel CreateGrpcChannel()
    {
        Assert.SkipWhen(ServerAddress is null, $"The {WebsiteUrl} environment variable is not set or is not a valid absolute URI.");
        return GrpcChannel.ForAddress(ServerAddress);
    }
}
