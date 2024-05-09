﻿// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Text;
using MartinCostello.Api.Extensions;
using MartinCostello.Api.Models;
using MartinCostello.Api.Options;
using Microsoft.Extensions.Options;

namespace MartinCostello.Api;

/// <summary>
/// A class for rendering HTML content. This class cannot be inherited.
/// </summary>
internal static class HtmlRendering
{
    private static ConcurrentDictionary<string, CompositeFormat> Templates { get; } = new();

    /// <summary>
    /// Returns the HTML for the API documentation page.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/>.</param>
    /// <returns>
    /// The rendered HTML for the page.
    /// </returns>
    public static string Docs(HttpContext context)
    {
        var renderingContext = CreateContext(
            context,
            "API Documentation",
            "Documentation for the resources in api.martincostello.com.");

        renderingContext.Links = $"""<link rel="swagger" href="{context.Request.Content("~/swagger/api/swagger.json", appendVersion: false)}" />""";
        renderingContext.Scripts = LoadTemplate("docs.scripts");
        renderingContext.Styles = LoadTemplate("docs.styles");

        string body = LoadTemplate("docs");

        return Layout(context, renderingContext, body);
    }

    /// <summary>
    /// Returns the HTML for the home page.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/>.</param>
    /// <returns>
    /// The rendered HTML for the page.
    /// </returns>
    public static string Home(HttpContext context)
    {
        var renderingContext = CreateContext(context, "Home Page");

        object?[] args =
        [
            Environment.Version.ToString(1),
            renderingContext.Options.Metadata?.Repository,
            System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
            renderingContext.Options.Metadata?.Author?.Website,
            renderingContext.Options.ExternalLinks?.Blog?.AbsoluteUri,
        ];

        string body = LoadTemplate("home", args);

        return Layout(context, renderingContext, body);
    }

    /// <summary>
    /// Returns the HTML for the error page.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/>.</param>
    /// <returns>
    /// The rendered HTML for the page.
    /// </returns>
    public static string Error(HttpContext context)
    {
        var renderingContext = CreateContext(context, "Error");

        int statusCode = StatusCodes.Status500InternalServerError;

        var id = context.Request.Query["id"];

        if (id is { Count: 1 } &&
            int.TryParse(id, CultureInfo.InvariantCulture, out int status) &&
            status >= StatusCodes.Status400BadRequest &&
            status < 600)
        {
            statusCode = status;
        }

        context.Response.StatusCode = statusCode;

        string body = LoadTemplate("error", statusCode);

        return Layout(context, renderingContext, body);
    }

    private static RenderingContext CreateContext(HttpContext context, string title, string? description = null)
    {
        string canonicalUri = context.Request.Canonical();
        var options = context.RequestServices.GetRequiredService<IOptions<SiteOptions>>().Value;

        return new RenderingContext()
        {
            CanonicalUri = canonicalUri,
            Description = description,
            Options = options,
            Title = title,
        };
    }

    private static string Layout(
        HttpContext context,
        RenderingContext renderingContext,
        string body)
    {
        var environment = context.RequestServices.GetRequiredService<IHostEnvironment>();

        // lang=html
        return
            $$"""
              <!DOCTYPE html>
              <html lang="en-gb">
              <head prefix="og:http://ogp.me/ns#">
                  {{Meta(context.Request, renderingContext, environment)}}
                  {{Links(context.Request, renderingContext)}}
                  {{renderingContext.Links}}
                  {{renderingContext.Styles}}
                  <script type="text/javascript">
                      if (self == top) {
                          document.documentElement.className = document.documentElement.className.replace(/\bjs-flash\b/, '');
                      }
                      else {
                          top.location = self.location;
                      }
                  </script>
              </head>
              <body>
                  {{Navbar(renderingContext.Options)}}
                  <main class="container body-content">
                      {{body}}
                      {{Footer(renderingContext.Options)}}
                  </main>
                  {{Styles(context.Request)}}
                  {{Scripts(context.Request, renderingContext.Options)}}
                  {{renderingContext.Scripts}}
              </body>
              <!--
                  Commit: {{GitMetadata.Commit}}
              -->
              </html>
              """;
    }

    private static string Footer(SiteOptions options)
    {
        object?[] args =
        [
            options.Metadata?.Author?.Name,
            DateTimeOffset.UtcNow.Year,
            options.Metadata?.Repository,
            GitMetadata.Commit,
            string.Join(string.Empty, GitMetadata.Commit.Take(7)),
            GitMetadata.Branch,
            GitMetadata.DeployId,
            GitMetadata.Timestamp.ToString("u", CultureInfo.InvariantCulture),
        ];

        return LoadTemplate("_footer", args);
    }

    private static string Links(HttpRequest request, RenderingContext context)
    {
        // lang=html
        return
            $"""
             <link rel="canonical" href="{context.CanonicalUri}" />
             <link rel="manifest" href="{request.Content("~/manifest.webmanifest")}" />
             <link rel="sitemap" type="application/xml" href="{request.Content("~/sitemap.xml")}" />
             <link rel="shortcut icon" type="image/x-icon" href="{request.CdnContent("favicon.ico", context.Options)}" />
             <link rel="apple-touch-icon" sizes="57x57" href="{request.CdnContent("apple-touch-icon-57x57.png", context.Options)}" />
             <link rel="apple-touch-icon" sizes="60x60" href="{request.CdnContent("apple-touch-icon-60x60.png", context.Options)}" />
             <link rel="apple-touch-icon" sizes="72x72" href="{request.CdnContent("apple-touch-icon-72x72.png", context.Options)}" />
             <link rel="apple-touch-icon" sizes="76x76" href="{request.CdnContent("apple-touch-icon-76x76.png", context.Options)}" />
             <link rel="apple-touch-icon" sizes="114x114" href="{request.CdnContent("apple-touch-icon-114x114.png", context.Options)}" />
             <link rel="apple-touch-icon" sizes="120x120" href="{request.CdnContent("apple-touch-icon-120x120.png", context.Options)}" />
             <link rel="apple-touch-icon" sizes="144x144" href="{request.CdnContent("apple-touch-icon-144x144.png", context.Options)}" />
             <link rel="apple-touch-icon" sizes="152x152" href="{request.CdnContent("apple-touch-icon-152x152.png", context.Options)}" />
             <link rel="apple-touch-icon" sizes="180x180" href="{request.CdnContent("apple-touch-icon-180x180.png", context.Options)}" />
             <link rel="icon" type="image/png" href="{request.CdnContent("android-chrome-192x192.png", context.Options)}" sizes="192x192" />
             <link rel="icon" type="image/png" href="{request.CdnContent("favicon-32x32.png", context.Options)}" sizes="32x32" />
             <link rel="icon" type="image/png" href="{request.CdnContent("favicon-96x96.png", context.Options)}" sizes="96x96" />
             <link rel="icon" type="image/png" href="{request.CdnContent("favicon-16x16.png", context.Options)}" sizes="16x16" />
             <link rel="dns-prefetch" href="//ajax.googleapis.com" />
             <link rel="dns-prefetch" href="//{context.Options.ExternalLinks?.Cdn?.Host}" />
             <link rel="dns-prefetch" href="//cdnjs.cloudflare.com" />
             <link rel="dns-prefetch" href="//fonts.googleapis.com" />
             <link rel="dns-prefetch" href="//fonts.gstatic.com" />
             <link rel="dns-prefetch" href="//www.googletagmanager.com" />
             <link rel="preconnect" href="//ajax.googleapis.com" />
             <link rel="preconnect" href="//{context.Options.ExternalLinks?.Cdn?.Host}" />
             <link rel="preconnect" href="//cdnjs.cloudflare.com" />
             <link rel="preconnect" href="//fonts.googleapis.com" />
             <link rel="preconnect" href="//fonts.gstatic.com" />
             <link rel="preconnect" href="//www.googletagmanager.com" />
             """;
    }

    private static string Meta(
        HttpRequest request,
        RenderingContext context,
        IHostEnvironment environment)
    {
        var model = MetaModel.Create(
            context.Options.Metadata,
            canonicalUri: context.CanonicalUri,
            description: context.Description,
            title: context.Title);

        // lang=html
        return
            $"""
             <title>{model.Title}</title>
             <meta charset="utf-8" />
             <meta http-equiv="cache-control" content="no-cache, no-store" />
             <meta http-equiv="content-type" content="text/html;" />
             <meta http-equiv="pragma" content="no-cache" />
             <meta http-equiv="X-UA-Compatible" content="IE=edge" />
             <meta name="author" content="{model.Author}" />
             <meta name="copyright" content="&copy; {model.Author} {DateTimeOffset.UtcNow.Date.Year}" />
             <meta name="description" content="{model.Description}" />
             <meta name="language" content="en" />
             <meta name="keywords" content="{model.Keywords}" />
             <meta name="referrer" content="origin-when-cross-origin" />
             <meta name="robots" content="{model.Robots}" />
             <meta name="theme-color" content="#ffffff" />
             <meta name="viewport" content="width=device-width, initial-scale=1.0" />
             <meta property="fb:profile_id" content="{model.Facebook}" />
             <meta property="og:description" content="{model.Description}" />
             <meta property="og:locale" content="en_GB" />
             <meta property="og:site_name" content="{model.SiteName}" />
             <meta property="og:title" content="{model.Title}" />
             <meta property="og:type" content="{model.SiteType}" />
             <meta property="og:url" content="{model.CanonicalUri}" />
             <meta name="twitter:card" content="{model.TwitterCard}" />
             <meta name="twitter:creator" content="{model.TwitterHandle}" />
             <meta name="twitter:description" content="{model.Description}" />
             <meta name="twitter:domain" content="{model.HostName}" />
             <meta name="twitter:site" content="{model.TwitterHandle}" />
             <meta name="twitter:title" content="{model.Title}" />
             <meta name="twitter:url" content="{model.CanonicalUri}" />
             <meta name="application-name" content="{model.SiteName}" />
             <meta name="google-analytics" content="{(environment.IsProduction() ? context.Options.Analytics?.Google : string.Empty)}" />
             <meta name="msapplication-config" content="browserconfig.xml" />
             <meta name="msapplication-navbutton-color" content="#0095DA" />
             <meta name="msapplication-starturl" content="/" />
             <meta name="msapplication-TileColor" content="#2d89ef" />
             <meta name="msapplication-TileImage" content="{request.CdnContent("mstile-144x144.png", context.Options)}" />
             """;
    }

    private static string Navbar(SiteOptions options)
    {
        object?[] args =
        [
            options.Metadata?.Domain,
            options.Api?.Documentation?.Location,
            options.ExternalLinks?.Blog?.AbsoluteUri,
            new Uri(new Uri(options.Metadata?.Author?.Website ?? string.Empty), "home/about/"),
        ];

        return LoadTemplate("_navbar", args);
    }

    private static string Scripts(HttpRequest request, SiteOptions options)
    {
        string? analyticsId = options.Analytics?.Google;
        string analyticsScript = string.Empty;

        if (!string.IsNullOrWhiteSpace(analyticsId))
        {
            analyticsScript = $"""<script src="https://www.googletagmanager.com/gtag/js?id={analyticsId}" async></script>""";
        }

        object?[] args =
        [
            analyticsScript,
            request.Content("~/assets/js/main.js"),
        ];

        return LoadTemplate("_scripts", args);
    }

    private static string Styles(HttpRequest request)
        => LoadTemplate("_styles", request.Content("~/assets/css/main.css"));

    private static string LoadTemplate(string name, params object?[] args)
    {
        if (!Templates.TryGetValue(name, out var format))
        {
            var type = typeof(ApiBuilder);
            var assembly = type.Assembly;

            string resource = $"{type.Namespace}.Templates.{name}.html";
            using var stream = assembly.GetManifestResourceStream(resource);

            string template;

            if (stream is null)
            {
                template = string.Empty;
            }
            else
            {
                using var reader = new StreamReader(stream);
                template = reader.ReadToEnd();
            }

            format = CompositeFormat.Parse(template);
            _ = Templates.TryAdd(name, format);
        }

        return string.Format(CultureInfo.InvariantCulture, format, args);
    }

    private struct RenderingContext
    {
        public SiteOptions Options { get; init; }

        public string CanonicalUri { get; init; }

        public string Title { get; init; }

        public string? Description { get; set; }

        public string? Links { get; set; }

        public string? Scripts { get; set; }

        public string? Styles { get; set; }
    }
}
