// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using MartinCostello.Api.Options;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace MartinCostello.Api.OpenApi;

/// <summary>
/// A class that adds API information. This class cannot be inherited.
/// </summary>
internal sealed class AddApiInfo(IOptions<SiteOptions> options) : IOpenApiDocumentTransformer
{
    /// <inheritdoc/>
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        ConfigureInfo(document.Info);

        return Task.CompletedTask;
    }

    private void ConfigureInfo(OpenApiInfo info)
    {
        var siteOptions = options.Value;

        info.Description = siteOptions.Metadata?.Description;
        info.Title = siteOptions.Metadata?.Name;
        info.Version = string.Empty;

        info.Contact = new()
        {
            Name = siteOptions.Metadata?.Author?.Name,
        };

        if (siteOptions.Metadata?.Author?.Website is { } contactUrl)
        {
            info.Contact.Url = new(contactUrl);
        }

        info.License = new()
        {
            Name = siteOptions.Api?.License?.Name,
        };

        if (siteOptions.Api?.License?.Url is { } licenseUrl)
        {
            info.License.Url = new(licenseUrl);
        }
    }
}
