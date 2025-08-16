// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.IO.Compression;
using System.Net.Mime;
using MartinCostello.Api.Extensions;
using MartinCostello.Api.Middleware;
using MartinCostello.Api.Options;
using MartinCostello.Api.Slices;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MartinCostello.Api;

/// <summary>
/// A class that builds and configures the API.
/// </summary>
public static class ApiBuilder
{
    /// <summary>
    /// Configures the specified <see cref="WebApplicationBuilder"/> and returns the created <see cref="WebApplication"/>.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> to use.</param>
    /// <returns>
    /// The built <see cref="WebApplication"/>.
    /// </returns>
    public static WebApplication Configure(WebApplicationBuilder builder)
    {
        builder.Services.AddOptions();
        builder.Services.Configure<SiteOptions>(builder.Configuration.GetSection("Site"));

        builder.Services.AddTelemetry(builder.Environment);

        builder.Services.AddAntiforgery((options) =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.Name = "_anti-forgery";
            options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
            options.FormFieldName = "_anti-forgery";
            options.HeaderName = "x-anti-forgery";
        });

        builder.Services.AddCors();
        builder.Services.Configure<CorsOptions>((corsOptions) =>
        {
            var siteOptions = new SiteOptions();
            builder.Configuration.Bind("Site", siteOptions);

            corsOptions.AddPolicy(
                "DefaultCorsPolicy",
                (policy) =>
                {
                    policy.WithExposedHeaders(siteOptions.Api?.Cors?.ExposedHeaders ?? [])
                          .WithHeaders(siteOptions.Api?.Cors?.Headers ?? [])
                          .WithMethods(siteOptions.Api?.Cors?.Methods ?? []);

                    if (siteOptions.Api?.Cors?.AllowAnyOrigin is true)
                    {
                        policy.AllowAnyOrigin();
                    }
                    else
                    {
                        policy.WithOrigins(siteOptions.Api?.Cors?.Origins ?? []);
                    }
                });
        });

        builder.Services.AddGrpc();

        builder.Services.Configure<BrotliCompressionProviderOptions>((p) => p.Level = CompressionLevel.Fastest);
        builder.Services.Configure<GzipCompressionProviderOptions>((p) => p.Level = CompressionLevel.Fastest);

        builder.Services.ConfigureHttpJsonOptions((options) =>
        {
            options.SerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.Strict;
            options.SerializerOptions.PropertyNameCaseInsensitive = false;
            options.SerializerOptions.WriteIndented = true;
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, ApplicationJsonSerializerContext.Default);
        });

        builder.Services.AddResponseCompression((options) =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });

        builder.Services.AddValidation();

        builder.Services.Configure<RouteOptions>((options) =>
        {
            options.AppendTrailingSlash = true;
            options.LowercaseUrls = true;
        });

        builder.Services.Configure<StaticFileOptions>((options) =>
        {
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".webmanifest"] = "application/manifest+json";

            options.ContentTypeProvider = provider;
            options.DefaultContentType = MediaTypeNames.Application.Json;
            options.ServeUnknownFileTypes = true;

            options.OnPrepareResponse = (context) =>
            {
                var maxAge = TimeSpan.FromDays(7);

                if (context.File.Exists && builder.Environment.IsProduction())
                {
                    string? extension = Path.GetExtension(context.File.PhysicalPath);

                    // These files are served with a content hash in the URL so can be cached for longer
                    bool isScriptOrStyle =
                        string.Equals(extension, ".css", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(extension, ".js", StringComparison.OrdinalIgnoreCase);

                    if (isScriptOrStyle)
                    {
                        maxAge = TimeSpan.FromDays(365);
                    }
                }

                var headers = context.Context.Response.GetTypedHeaders();
                headers.CacheControl = new() { MaxAge = maxAge };
            };
        });

        if (!builder.Environment.IsDevelopment())
        {
            builder.Services.AddHsts((options) =>
            {
                options.MaxAge = TimeSpan.FromDays(365);
                options.IncludeSubDomains = false;
                options.Preload = false;
            });
        }

        builder.Services.AddHttpClient();
        builder.Services.AddOpenApiDocumentation();
        builder.Services.AddOutputCache();

        builder.Services.TryAddSingleton(TimeProvider.System);

        builder.Logging.AddTelemetry();

        builder.WebHost.CaptureStartupErrors(true);
        builder.WebHost.ConfigureKestrel((options) =>
        {
            options.AddServerHeader = false;

            if (builder.Configuration["ASPNETCORE_HTTP_PORTS"] is { Length: > 0 } httpPort &&
                int.TryParse(httpPort, CultureInfo.InvariantCulture, out int port))
            {
                options.ListenAnyIP(port);
            }

            if (builder.Configuration["HTTP20_ONLY_PORT"] is { Length: > 0 } http20Port &&
                int.TryParse(http20Port, CultureInfo.InvariantCulture, out port))
            {
                options.ListenAnyIP(port, (p) => p.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2);
            }
        });

        if (builder.Configuration["Sentry:Dsn"] is { Length: > 0 } dsn)
        {
            builder.WebHost.UseSentry(dsn);
        }

        var app = builder.Build();

        if (ApplicationTelemetry.IsPyroscopeConfigured())
        {
            app.UseMiddleware<PyroscopeK6Middleware>();
        }

        app.UseMiddleware<CustomHttpHeadersMiddleware>();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/error");
        }

        app.UseStatusCodePagesWithReExecute("/error", "?id={0}");

        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();

            if (!string.Equals(app.Configuration["ForwardedHeaders_Enabled"], bool.TrueString, StringComparison.OrdinalIgnoreCase))
            {
                app.UseHttpsRedirection();
            }
        }

        app.UseResponseCompression();

        app.UseCors();
        app.UseOutputCache();

        app.MapOpenApi().CacheOutput();
        app.MapOpenApi("/openapi/{documentName}.yaml").CacheOutput();

        app.UseStaticFiles();

        app.UseCookiePolicy(new()
        {
            HttpOnly = HttpOnlyPolicy.Always,
            Secure = app.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always,
        });

        app.MapApiEndpoints();
        app.MapGitHubEndpoints();
        app.MapGrpcService<TimeGrpcService>();

        string[] methods = [HttpMethod.Get.Method, HttpMethod.Head.Method];

        app.MapMethods("/", methods, () => Results.Extensions.RazorSlice<Home>())
           .ExcludeFromDescription();

        app.MapMethods("/docs", methods, () => Results.Extensions.RazorSlice<Docs>())
           .ExcludeFromDescription();

        app.MapMethods("/error", methods, (int? id) =>
        {
            int statusCode = StatusCodes.Status500InternalServerError;

            if (id is { } status &&
                status >= 400 && status < 599)
            {
                statusCode = status;
            }

            return Results.Extensions.RazorSlice<Error>(statusCode);
        }).ExcludeFromDescription();

        return app;
    }
}
