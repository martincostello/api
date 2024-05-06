// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

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

        renderingContext.Links =
            $"""
             <link rel="swagger" href="{context.Request.Content("~/swagger/api/swagger.json")}" />
             """;

        renderingContext.Scripts =
            """
            <script src="https://cdnjs.cloudflare.com/ajax/libs/swagger-ui/5.17.3/swagger-ui-bundle.min.js" integrity="sha512-utyd7bY+4W4mRdhMMFcK3/gF0cohNI6jAOl6Sxi7uQYoCM/KzcGOYTNHJgG5eWR1IKN7dQPDCJ403PiXAwOHXw==" crossorigin="anonymous" referrerpolicy="no-referrer" defer></script>
            <script src="https://cdnjs.cloudflare.com/ajax/libs/swagger-ui/5.17.3/swagger-ui-standalone-preset.min.js" integrity="sha512-O9wzlLwJTuO3Rt8s5Dpn750A4KyhyUvG4YDgUMyFXLZ+v15MZuWsYeJfJIMWR2iwwlgIsgpkHdzr2LiBRNfn7g==" crossorigin="anonymous" referrerpolicy="no-referrer" defer></script>
            """;

        renderingContext.Styles =
            """
            <link rel="stylesheet" href="https://fonts.googleapis.com/css?family=Open+Sans:400,700|Source+Code+Pro:300,600|Titillium+Web:400,600,700" crossorigin="anonymous" />
            <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/swagger-ui/5.17.3/swagger-ui.min.css" integrity="sha512-+9UD8YSD9GF7FzOH38L9S6y56aYNx3R4dYbOCgvTJ2ZHpJScsahNdaMQJU/8osUiz9FPu0YZ8wdKf4evUbsGSg==" crossorigin="anonymous" referrerpolicy="no-referrer" />
            """;

        string body =
            """
            <h1>API Documentation</h1>
            <div id="swagger-ui"></div>
            <svg class="swagger-ui-svg" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink">
                <defs>
                    <symbol viewBox="0 0 20 20" id="unlocked">
                        <path d="M15.8 8H14V5.6C14 2.703 12.665 1 10 1 7.334 1 6 2.703 6 5.6V6h2v-.801C8 3.754 8.797 3 10 3c1.203 0 2 .754 2 2.199V8H4c-.553 0-1 .646-1 1.199V17c0 .549.428 1.139.951 1.307l1.197.387C5.672 18.861 6.55 19 7.1 19h5.8c.549 0 1.428-.139 1.951-.307l1.196-.387c.524-.167.953-.757.953-1.306V9.199C17 8.646 16.352 8 15.8 8z"></path>
                    </symbol>
                    <symbol viewBox="0 0 20 20" id="locked">
                        <path d="M15.8 8H14V5.6C14 2.703 12.665 1 10 1 7.334 1 6 2.703 6 5.6V8H4c-.553 0-1 .646-1 1.199V17c0 .549.428 1.139.951 1.307l1.197.387C5.672 18.861 6.55 19 7.1 19h5.8c.549 0 1.428-.139 1.951-.307l1.196-.387c.524-.167.953-.757.953-1.306V9.199C17 8.646 16.352 8 15.8 8zM12 8H8V5.199C8 3.754 8.797 3 10 3c1.203 0 2 .754 2 2.199V8z" />
                    </symbol>
                    <symbol viewBox="0 0 20 20" id="close">
                        <path d="M14.348 14.849c-.469.469-1.229.469-1.697 0L10 11.819l-2.651 3.029c-.469.469-1.229.469-1.697 0-.469-.469-.469-1.229 0-1.697l2.758-3.15-2.759-3.152c-.469-.469-.469-1.228 0-1.697.469-.469 1.228-.469 1.697 0L10 8.183l2.651-3.031c.469-.469 1.228-.469 1.697 0 .469.469.469 1.229 0 1.697l-2.758 3.152 2.758 3.15c.469.469.469 1.229 0 1.698z" />
                    </symbol>
                    <symbol viewBox="0 0 20 20" id="large-arrow">
                        <path d="M13.25 10L6.109 2.58c-.268-.27-.268-.707 0-.979.268-.27.701-.27.969 0l7.83 7.908c.268.271.268.709 0 .979l-7.83 7.908c-.268.271-.701.27-.969 0-.268-.269-.268-.707 0-.979L13.25 10z" />
                    </symbol>
                    <symbol viewBox="0 0 20 20" id="large-arrow-down">
                        <path d="M17.418 6.109c.272-.268.709-.268.979 0s.271.701 0 .969l-7.908 7.83c-.27.268-.707.268-.979 0l-7.908-7.83c-.27-.268-.27-.701 0-.969.271-.268.709-.268.979 0L10 13.25l7.418-7.141z" />
                    </symbol>
                    <symbol viewBox="0 0 24 24" id="jump-to">
                        <path d="M19 7v4H5.83l3.58-3.59L8 6l-6 6 6 6 1.41-1.41L5.83 13H21V7z" />
                    </symbol>
                    <symbol viewBox="0 0 24 24" id="expand">
                        <path d="M10 18h4v-2h-4v2zM3 6v2h18V6H3zm3 7h12v-2H6v2z" />
                    </symbol>
                </defs>
            </svg>
            """;

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

        // lang=html
        string body =
            $"""
             <div class="bg-light mb-4 p-5 rounded-3">
                 <div class="container-fluid pt-2">
                     <h1 class="display-5 fw-bold">Martin Costello's API</h1>
                     <p class="col-md-8 fs-4">
                         This website is an excercise in the use of ASP.NET Core {Environment.Version.ToString(1)} for a website and REST API.
                     </p>
                 </div>
             </div>
             <div>
                 <p>
                     This website is hosted in <a href="https://azure.microsoft.com" rel="noopener" target="_blank" title="Microsoft Azure">Microsoft Azure</a>
                     and the source code can be found on <a href="{renderingContext.Options.Metadata?.Repository}" rel="noopener" target="_blank" title="This application's GitHub repository">GitHub</a>.
                 </p>
                 <p>
                     It is currently running {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}.
                 </p>
             </div>
             <div class="row">
                 <div class="col-lg-6">
                     <h2 class="h3">Website</h2>
                     <p>My main website.</p>
                     <p><a id="link-website" href="{renderingContext.Options.Metadata?.Author?.Website}" class="btn btn-lg btn-primary" rel="noopener" role="button" target="_blank" title="Visit my website">Visit website »</a></p>
                 </div>
                 <div class="col-lg-6">
                     <h2 class="h3">Blog</h2>
                     <p>I occasionally blog about topics related to .NET development.</p>
                     <p><a id="link-blog" href="{renderingContext.Options.ExternalLinks?.Blog?.AbsoluteUri}" class="btn btn-lg btn-primary" rel="noopener" role="button" target="_blank" title="Visit my blog">Visit blog »</a></p>
                 </div>
             </div>
             """;

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

        // lang=html
        string body =
            $"""
             <h1 class="text-danger">Error.</h1>
             <h2 class="text-danger">An error occurred while processing your request.</h2>
             <h3 class="text-danger">HTTP status code {statusCode}.</h3>
             """;

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
        string? analyticsId = renderingContext.Options.Analytics?.Google;

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
                  <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/bootswatch/5.3.3/flatly/bootstrap.min.css" integrity="sha512-qoT4KwnRpAQ9uczPsw7GunsNmhRnYwSlE2KRCUPRQHSkDuLulCtDXuC2P/P6oqr3M5hoGagUG9pgHDPkD2zCDA==" crossorigin="anonymous" referrerpolicy="no-referrer" />
                  <link rel="stylesheet" href="{{context.Request.Content("~/assets/css/main.css")}}" />
                  <script src="https://cdnjs.cloudflare.com/ajax/libs/bootstrap/5.3.3/js/bootstrap.bundle.min.js" integrity="sha512-7Pi/otdlbbCR+LnW+F7PwFcSDJOuUJB3OxtEHbg4vSMvzvJjde4Po1v4BR9Gdc9aXNUNFVUY+SK51wWT8WF0Gg==" crossorigin="anonymous" referrerpolicy="no-referrer" defer></script>
                  <script src="https://cdnjs.cloudflare.com/ajax/libs/jquery/3.7.1/jquery.min.js" integrity="sha512-v2CJ7UaYy4JwqLDIrZUI/4hqeoQieOmAZNXBeQyjo21dadnwR+8ZaIJVT8EE2iyI61OV8e6M8PP2/4hpQINQ/g==" crossorigin="anonymous" referrerpolicy="no-referrer" defer></script>
                  <script src="https://cdnjs.cloudflare.com/ajax/libs/jquery.lazyload/1.9.1/jquery.lazyload.min.js" integrity="sha512-jNDtFf7qgU0eH/+Z42FG4fw3w7DM/9zbgNPe3wfJlCylVDTT3IgKW5r92Vy9IHa6U50vyMz5gRByIu4YIXFtaQ==" crossorigin="anonymous" referrerpolicy="no-referrer" defer></script>
                  {{(string.IsNullOrEmpty(analyticsId) ? string.Empty : $"<script src=\"https://www.googletagmanager.com/gtag/js?id={analyticsId}\" async></script>")}}
                  <script src="{{context.Request.Content("~/assets/js/main.js")}}" defer></script>
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
        // lang=html
        return
            $"""
             <hr />
             <footer>
                 <p>
                     &copy; {options.Metadata?.Author?.Name} {DateTimeOffset.UtcNow.Year} |
                     Built from
                     <a href="{options.Metadata?.Repository}/commit/{GitMetadata.Commit}" title="View commit {GitMetadata.Commit} on GitHub">
                         {string.Join(string.Empty, GitMetadata.Commit.Take(7))}
                     </a>
                     on
                     <a href="{options.Metadata?.Repository}/tree/{GitMetadata.Branch}" title="View branch {GitMetadata.Branch} on GitHub">
                         {GitMetadata.Branch}
                     </a>
                     by
                     <a href="{options.Metadata?.Repository}/actions/runs/{GitMetadata.DeployId}" title="View deployment on GitHub">
                         GitHub
                     </a>
                     <span id="build-date" data-format="YYYY-MM-DD HH:mm Z" data-timestamp="{GitMetadata.Timestamp.ToString("u", CultureInfo.InvariantCulture)}"></span>
                 </p>
             </footer>
             """;
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
        // lang=html
        return
            $"""
             <nav class="navbar navbar-expand-lg fixed-top navbar-dark bg-primary">
                 <div class="container">
                     <a id="link-home" href="/" class="navbar-brand">{options.Metadata?.Domain}</a>
                     <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#site-navbar" aria-controls="site-navbar" aria-expanded="false" aria-label="Toggle navigation">
                         <span class="navbar-toggler-icon"></span>
                     </button>
                     <div class="collapse navbar-collapse" id="site-navbar">
                         <ul class="navbar-nav me-auto">
                             <li class="nav-item">
                                 <a id="link-docs" href="/{options.Api?.Documentation?.Location}" class="nav-link" title="My personal projects">Documentation</a>
                             </li>
                             <li class="nav-item">
                                 <a id="link-blog-header" class="nav-link" href="{options.ExternalLinks?.Blog?.AbsoluteUri}" rel="noopener" target="_blank" title="My blog">Blog</a>
                             </li>
                             <li class="nav-item">
                                 <a id="link-about" class="nav-link" href="{new Uri(new Uri(options.Metadata?.Author?.Website ?? string.Empty), "home/about/")}" rel="noopener" target="_blank" title="About me">About</a>
                             </li>
                         </ul>
                     </div>
                 </div>
             </nav>
             """;
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
