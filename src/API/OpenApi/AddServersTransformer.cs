﻿// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace MartinCostello.Api.OpenApi;

/// <summary>
/// A class representing a document processor that server information. This class cannot be inherited.
/// </summary>
/// <param name="options">The configured <see cref="ForwardedHeadersOptions"/>.</param>
internal sealed class AddServersTransformer(IOptions<ForwardedHeadersOptions> options) : IOpenApiDocumentTransformer
{
    /// <inheritdoc/>
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Servers = [new() { Url = GetServerUrl(context, options.Value) }];
        return Task.CompletedTask;
    }

    private static string GetServerUrl(OpenApiDocumentTransformerContext context, ForwardedHeadersOptions options)
    {
        // TODO Use context.HttpContext if https://github.com/dotnet/aspnetcore/issues/56189 is implemented
        var accessor = context.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
        var request = accessor.HttpContext!.Request;

        string scheme = TryGetFirstHeader(options.ForwardedProtoHeaderName) ?? request.Scheme;
        string host = TryGetFirstHeader(options.ForwardedHostHeaderName) ?? request.Host.ToString();

        return new Uri($"{scheme}://{host}").ToString().TrimEnd('/');

        string? TryGetFirstHeader(string name)
            => request.Headers.TryGetValue(name, out var values) ? values.FirstOrDefault() : null;
    }
}