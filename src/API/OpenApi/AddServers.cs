// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MartinCostello.Api.OpenApi;

/// <summary>
/// A class that server information to an OpenAPI document. This class cannot be inherited.
/// </summary>
/// <param name="accessor">The <see cref="IHttpContextAccessor"/> to use.</param>
/// <param name="options">The configured <see cref="ForwardedHeadersOptions"/>.</param>
internal sealed class AddServers(
    IHttpContextAccessor accessor,
    IOptions<ForwardedHeadersOptions> options) : IDocumentFilter
{
    /// <inheritdoc/>
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Servers = [new() { Url = GetServerUrl(accessor, options.Value) }];
    }

    private static string GetServerUrl(IHttpContextAccessor accessor, ForwardedHeadersOptions options)
    {
        var request = accessor.HttpContext!.Request;

        string scheme = TryGetFirstHeader(options.ForwardedProtoHeaderName) ?? request.Scheme;
        string host = TryGetFirstHeader(options.ForwardedHostHeaderName) ?? request.Host.ToString();

        return new Uri($"{scheme}://{host}").ToString().TrimEnd('/');

        string? TryGetFirstHeader(string name)
            => request.Headers.TryGetValue(name, out var values) ? values.FirstOrDefault() : null;
    }
}
