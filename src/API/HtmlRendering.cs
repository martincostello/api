// Copyright (c) Martin Costello, 2016. All rights reserved.
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

        object?[] args =
        [
            Meta(context.Request, renderingContext, environment),
            Links(context.Request, renderingContext),
            renderingContext.Links,
            renderingContext.Styles,
            Navbar(renderingContext.Options),
            body,
            Footer(renderingContext.Options),
            Styles(context.Request),
            Scripts(context.Request, renderingContext.Options),
            renderingContext.Scripts,
            GitMetadata.Commit,
        ];

        return LoadTemplate("_layout", args);
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
        var builder = new StringBuilder();

        AppendLink(builder, "canonical", context.CanonicalUri);
        AppendLink(builder, "manifest", request.Content("~/manifest.webmanifest"));
        AppendLink(builder, "sitemap", request.Content("~/sitemap.xml"), type: "application/xml");
        AppendLink(builder, "shortcut icon", request.CdnContent("favicon.ico", context.Options), type: "image/x-icon");

        ReadOnlySpan<string> sizes = ["57", "60", "72", "76", "114", "120", "144", "152", "180"];

        foreach (string size in sizes)
        {
            AppendLink(builder, "apple-touch-icon", request.CdnContent($"apple-touch-icon-{size}x{size}.png", context.Options), sizes: $"{size}x{size}");
        }

        AppendLink(builder, "icon", request.CdnContent("android-chrome-192x192.png", context.Options), type: "image/png", sizes: "192x192");

        sizes = ["16", "32", "96"];

        foreach (string size in sizes)
        {
            AppendLink(builder, "icon", request.CdnContent($"favicon-{size}x{size}.png", context.Options), type: "image/png", sizes: $"{size}x{size}");
        }

        ReadOnlySpan<string> domains =
        [
            "//ajax.googleapis.com",
            $"//{context.Options.ExternalLinks?.Cdn?.Host}",
            "//cdnjs.cloudflare.com",
            "//fonts.googleapis.com",
            "//fonts.gstatic.com",
            "//www.googletagmanager.com",
        ];

        foreach (string domain in domains)
        {
            AppendLink(builder, "dns-prefetch", domain);
            AppendLink(builder, "preconnect", domain);
        }

        return builder.ToString();

        static void AppendLink(StringBuilder builder, string rel, string? href, string? type = null, string? sizes = null)
        {
            builder.Append($"<link rel=\"{rel}\" href=\"{href}\" ");

            if (type is not null)
            {
                builder.Append($"type=\"{type}\" ");
            }

            if (sizes is not null)
            {
                builder.Append($"sizes=\"{sizes}\" ");
            }

            builder.AppendLine("/>");
        }
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
            using var stream = assembly.GetManifestResourceStream(resource) ?? throw new ArgumentException($"The '{name}' template cannot be found.", nameof(name));
            using var reader = new StreamReader(stream);
            string template = reader.ReadToEnd();

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
