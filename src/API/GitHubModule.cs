// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace MartinCostello.Api;

#pragma warning disable CA2234

/// <summary>
/// A class that configures the GitHub endpoints.
/// </summary>
public static class GitHubModule
{
    /// <summary>
    /// Maps the GitHub endpoints.
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointRouteBuilder"/> to use.</param>
    /// <returns>
    /// The <see cref="IEndpointRouteBuilder"/> specified by <paramref name="builder"/>.
    /// </returns>
    public static IEndpointRouteBuilder MapGitHubEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("github")
                           .RequireCors("DefaultCorsPolicy")
                           .ExcludeFromDescription();

        // See https://docs.github.com/apps/oauth-apps/building-oauth-apps/authorizing-oauth-apps#step-1-app-requests-the-device-and-user-verification-codes-from-github
        group.MapPost("login/device/code", async (
            [FromServices] HttpClient client,
            [FromQuery(Name = "client_id")] string clientId,
            [FromQuery] string scope,
            CancellationToken cancellationToken) =>
        {
            var parameters = new Dictionary<string, string?>(2)
            {
                ["client_id"] = clientId,
                ["scope"] = scope,
            };

            string requestUri = QueryHelpers.AddQueryString("https://github.com/login/device/code", parameters);

            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var response = await client.PostAsync(requestUri, null, cancellationToken);
            response.EnsureSuccessStatusCode();

            var deviceCode = await response.Content.ReadFromJsonAsync(
                ApplicationJsonSerializerContext.Default.GitHubDeviceCode,
                cancellationToken);

            return Results.Json(deviceCode);
        });

        // See https://docs.github.com/apps/oauth-apps/building-oauth-apps/authorizing-oauth-apps#step-3-app-polls-github-to-check-if-the-user-authorized-the-device
        group.MapPost("login/oauth/access_token", static async (
            [FromServices] HttpClient client,
            [FromQuery(Name = "client_id")] string clientId,
            [FromQuery(Name = "device_code")] string deviceCode,
            [FromQuery(Name = "grant_type")] string grantType,
            CancellationToken cancellationToken) =>
        {
            var parameters = new Dictionary<string, string?>(3)
            {
                ["client_id"] = clientId,
                ["device_code"] = deviceCode,
                ["grant_type"] = grantType,
            };

            string requestUri = QueryHelpers.AddQueryString("https://github.com/login/oauth/access_token", parameters);

            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var response = await client.PostAsync(requestUri, null, cancellationToken);
            response.EnsureSuccessStatusCode();

            var accessToken = await response.Content.ReadFromJsonAsync(
                ApplicationJsonSerializerContext.Default.GitHubAccessToken,
                cancellationToken);

            return Results.Json(accessToken);
        });

        return builder;
    }
}
