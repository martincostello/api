// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Net.Mime;
using MartinCostello.Api.Extensions;
using MartinCostello.Api.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using NodaTime;

namespace MartinCostello.Api
{
    /// <summary>
    /// A class representing the startup logic for the application.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// The name of the default CORS policy.
        /// </summary>
        internal const string DefaultCorsPolicyName = "DefaultCorsPolicy";

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="IConfiguration"/> to use.</param>
        /// <param name="hostingEnvironment">The <see cref="IWebHostEnvironment"/> to use.</param>
        public Startup(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// Gets the current configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Gets the current hosting environment.
        /// </summary>
        public IWebHostEnvironment HostingEnvironment { get; }

        /// <summary>
        /// Gets or sets the service provider.
        /// </summary>
        public IServiceProvider? ServiceProvider { get; set; }

        /// <summary>
        /// Configures the application.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to use.</param>
        /// <param name="options">The <see cref="IOptions{SiteOptions}"/> to use.</param>
        public void Configure(
            IApplicationBuilder app,
            IOptions<SiteOptions> options)
        {
            ServiceProvider = app.ApplicationServices;

            app.UseCustomHttpHeaders(HostingEnvironment, Configuration, options);

            if (HostingEnvironment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error")
                   .UseStatusCodePagesWithReExecute("/error", "?id={0}");

                app.UseHsts()
                   .UseHttpsRedirection();
            }

            app.UseForwardedHeaders(
                new ForwardedHeadersOptions()
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
                });

            app.UseHttpMethodOverride();

            app.UseResponseCompression();

            app.UseStaticFiles(CreateStaticFileOptions());

            app.UseRouting();

            app.UseCors();

            app.UseEndpoints(
                (endpoints) =>
                {
                    endpoints.MapDefaultControllerRoute();
                });

            app.UseSwagger();

            app.UseCookiePolicy(CreateCookiePolicy());
        }

        /// <summary>
        /// Configures the services for the application.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to use.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry(Configuration);

            services.AddOptions();
            services.Configure<SiteOptions>(Configuration.GetSection("Site"));

            services.AddAntiforgery(
                (p) =>
                {
                    p.Cookie.HttpOnly = true;
                    p.Cookie.Name = "_anti-forgery";
                    p.Cookie.SecurePolicy = CreateCookieSecurePolicy();
                    p.FormFieldName = "_anti-forgery";
                    p.HeaderName = "x-anti-forgery";
                });

            services.AddCors(ConfigureCors);

            services.AddControllersWithViews(ConfigureMvc)
                    .AddJsonOptions(ConfigureJsonFormatter);

            services.AddResponseCompression();

            services.AddRouting(
                (p) =>
                {
                    p.AppendTrailingSlash = true;
                    p.LowercaseUrls = true;
                });

            if (!HostingEnvironment.IsDevelopment())
            {
                services.AddHsts(
                    (p) =>
                    {
                        p.MaxAge = TimeSpan.FromDays(365);
                        p.IncludeSubDomains = false;
                        p.Preload = false;
                    });
            }

            services.AddSwagger(HostingEnvironment);
            services.AddSingleton<IClock>((_) => SystemClock.Instance);
        }

        /// <summary>
        /// Configures the JSON serializer for MVC.
        /// </summary>
        /// <param name="options">The <see cref="JsonOptions"/> to configure.</param>
        private static void ConfigureJsonFormatter(JsonOptions options)
        {
            // Make JSON easier to read for debugging at the expense of larger payloads
            options.JsonSerializerOptions.WriteIndented = true;

            // Opt-out of case insensitivity on property names
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = false;
        }

        /// <summary>
        /// Configures CORS.
        /// </summary>
        /// <param name="corsOptions">The <see cref="CorsOptions"/> to configure.</param>
        private void ConfigureCors(CorsOptions corsOptions)
        {
            var options = ServiceProvider!.GetService<IOptions<SiteOptions>>();
            var siteOptions = options!.Value;

            corsOptions.AddPolicy(
                DefaultCorsPolicyName,
                (builder) =>
                {
                    builder
                        .WithExposedHeaders(siteOptions.Api?.Cors?.ExposedHeaders ?? Array.Empty<string>())
                        .WithHeaders(siteOptions.Api?.Cors?.Headers ?? Array.Empty<string>())
                        .WithMethods(siteOptions.Api?.Cors?.Methods ?? Array.Empty<string>());

                    if (HostingEnvironment.IsDevelopment())
                    {
                        builder.AllowAnyOrigin();
                    }
                    else
                    {
                        builder.WithOrigins(siteOptions.Api?.Cors?.Origins ?? Array.Empty<string>());
                    }
                });
        }

        /// <summary>
        /// Configures MVC.
        /// </summary>
        /// <param name="options">The <see cref="MvcOptions"/> to configure.</param>
        private void ConfigureMvc(MvcOptions options)
        {
            if (!HostingEnvironment.IsDevelopment())
            {
                options.Filters.Add(new RequireHttpsAttribute());
            }
        }

        /// <summary>
        /// Creates the <see cref="CookiePolicyOptions"/> to use.
        /// </summary>
        /// <returns>
        /// The <see cref="CookiePolicyOptions"/> to use for the application.
        /// </returns>
        private CookiePolicyOptions CreateCookiePolicy()
        {
            return new CookiePolicyOptions()
            {
                HttpOnly = HttpOnlyPolicy.Always,
                Secure = CreateCookieSecurePolicy(),
            };
        }

        /// <summary>
        /// Creates the <see cref="CookieSecurePolicy"/> to use.
        /// </summary>
        /// <returns>
        /// The <see cref="CookieSecurePolicy"/> to use for the application.
        /// </returns>
        private CookieSecurePolicy CreateCookieSecurePolicy()
            => HostingEnvironment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;

        /// <summary>
        /// Configures the options for serving static content.
        /// </summary>
        /// <returns>
        /// The <see cref="StaticFileOptions"/> to use.
        /// </returns>
        private StaticFileOptions CreateStaticFileOptions()
        {
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".webmanifest"] = "application/manifest+json";

            return new StaticFileOptions()
            {
                ContentTypeProvider = provider,
                DefaultContentType = MediaTypeNames.Application.Json,
                OnPrepareResponse = SetCacheHeaders,
                ServeUnknownFileTypes = true,
            };
        }

        /// <summary>
        /// Sets the cache headers for static files.
        /// </summary>
        /// <param name="context">The static file response context to set the headers for.</param>
        private void SetCacheHeaders(StaticFileResponseContext context)
        {
            var maxAge = TimeSpan.FromDays(7);

            if (context.File.Exists && HostingEnvironment.IsProduction())
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
            headers.CacheControl = new CacheControlHeaderValue()
            {
                MaxAge = maxAge,
            };
        }
    }
}
