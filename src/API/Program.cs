// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

#pragma warning disable SA1516

using System.Net.Mime;
using MartinCostello.Api.Extensions;
using MartinCostello.Api.Options;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using NodaTime;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry(builder.Configuration);

builder.Services.AddOptions();
builder.Services.Configure<SiteOptions>(builder.Configuration.GetSection("Site"));

builder.Services.AddAntiforgery(
    (p) =>
    {
        p.Cookie.HttpOnly = true;
        p.Cookie.Name = "_anti-forgery";
        p.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
        p.FormFieldName = "_anti-forgery";
        p.HeaderName = "x-anti-forgery";
    });

builder.Services.AddCors((corsOptions) =>
{
    var siteOptions = new SiteOptions();
    builder.Configuration.Bind("Site", siteOptions);

    corsOptions.AddPolicy(
        "DefaultCorsPolicy",
        (policy) =>
        {
            policy
                .WithExposedHeaders(siteOptions.Api?.Cors?.ExposedHeaders ?? Array.Empty<string>())
                .WithHeaders(siteOptions.Api?.Cors?.Headers ?? Array.Empty<string>())
                .WithMethods(siteOptions.Api?.Cors?.Methods ?? Array.Empty<string>());

            if (builder.Environment.IsDevelopment())
            {
                policy.AllowAnyOrigin();
            }
            else
            {
                policy.WithOrigins(siteOptions.Api?.Cors?.Origins ?? Array.Empty<string>());
            }
        });
});

builder.Services.AddControllersWithViews(
    (options) =>
    {
        if (!builder.Environment.IsDevelopment())
        {
            options.Filters.Add(new RequireHttpsAttribute());
        }
    })
    .AddJsonOptions((options) =>
    {
        // Make JSON easier to read for debugging at the expense of larger payloads
        options.JsonSerializerOptions.WriteIndented = true;

        // Opt-out of case insensitivity on property names
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = false;
    });

builder.Services.AddResponseCompression();

builder.Services.AddRouting(
    (p) =>
    {
        p.AppendTrailingSlash = true;
        p.LowercaseUrls = true;
    });

if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddHsts(
        (p) =>
        {
            p.MaxAge = TimeSpan.FromDays(365);
            p.IncludeSubDomains = false;
            p.Preload = false;
        });
}

builder.Services.AddSwagger(builder.Environment);
builder.Services.AddSingleton<IClock>((_) => SystemClock.Instance);

builder.WebHost.CaptureStartupErrors(true);
builder.WebHost.ConfigureKestrel((p) => p.AddServerHeader = false);

var app = builder.Build();

app.UseCustomHttpHeaders(app.Environment, app.Configuration, app.Services.GetRequiredService<IOptions<SiteOptions>>());

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error")
       .UseStatusCodePagesWithReExecute("/error", "?id={0}");

    app.UseHsts()
       .UseHttpsRedirection();
}

app.UseResponseCompression();

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".webmanifest"] = "application/manifest+json";

app.UseStaticFiles(new StaticFileOptions()
{
    ContentTypeProvider = provider,
    DefaultContentType = MediaTypeNames.Application.Json,
    OnPrepareResponse = (context) => SetCacheHeaders(context, app.Environment.IsDevelopment()),
    ServeUnknownFileTypes = true,
});

app.UseRouting();

app.UseCors();

app.UseEndpoints(
    (endpoints) =>
    {
        endpoints.MapDefaultControllerRoute();
    });

app.UseSwagger();

app.UseCookiePolicy(new()
{
    HttpOnly = HttpOnlyPolicy.Always,
    Secure = app.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always,
});

app.Start();

static void SetCacheHeaders(StaticFileResponseContext context, bool isDevelopment)
{
    var maxAge = TimeSpan.FromDays(7);

    if (context.File.Exists && !isDevelopment)
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
}
