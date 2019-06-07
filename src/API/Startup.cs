// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api
{
    using System;
    using System.IO;
    using Extensions;
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
    using Microsoft.Extensions.Options;
    using Microsoft.Net.Http.Headers;
    using Newtonsoft.Json;
    using NodaTime;
    using Options;

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
        /// <param name="hostingEnvironment">The <see cref="IHostingEnvironment"/> to use.</param>
        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
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
        public IHostingEnvironment HostingEnvironment { get; }

        /// <summary>
        /// Gets or sets the service provider's scope.
        /// </summary>
        public IServiceScope ServiceScope { get; set; }

        /// <summary>
        /// Configures the application.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to use.</param>
        /// <param name="applicationLifetime">The <see cref="IApplicationLifetime"/> to use.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to use.</param>
        /// <param name="options">The <see cref="IOptions{SiteOptions}"/> to use.</param>
        public void Configure(
            IApplicationBuilder app,
            IApplicationLifetime applicationLifetime,
            IServiceProvider serviceProvider,
            IOptions<SiteOptions> options)
        {
            applicationLifetime.ApplicationStopped.Register(OnApplicationStopped);

            ServiceScope = serviceProvider.CreateScope();

            app.UseCustomHttpHeaders(HostingEnvironment, Configuration, options.Value);

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

            app.UseStaticFiles(CreateStaticFileOptions());

            app.UseMvcWithDefaultRoute();

            app.UseSwagger();

            app.UseCookiePolicy(CreateCookiePolicy());
        }

        /// <summary>
        /// Configures the services for the application.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to use.</param>
        public void ConfigureServices(IServiceCollection services)
        {
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

            services.AddMemoryCache()
                    .AddDistributedMemoryCache();

            services.AddCors(ConfigureCors);

            services
                .AddMvc(ConfigureMvc)
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions((p) => services.AddSingleton(ConfigureJsonFormatter(p)));

            services.AddRouting(
                (p) =>
                {
                    p.AppendTrailingSlash = true;
                    p.LowercaseUrls = true;
                });

            services.AddHsts(
                (p) =>
                {
                    p.MaxAge = TimeSpan.FromDays(365);
                    p.IncludeSubDomains = false;
                    p.Preload = false;
                });

            services.AddSwagger(HostingEnvironment);
            services.AddSingleton<IClock>((_) => SystemClock.Instance);
            services.AddSingleton((p) => p.GetRequiredService<IOptions<SiteOptions>>().Value);
            services.AddSingleton((_) => ConfigureJsonFormatter(new JsonSerializerSettings()));
        }

        /// <summary>
        /// Configures the JSON serializer for MVC.
        /// </summary>
        /// <param name="options">The <see cref="MvcJsonOptions"/> to configure.</param>
        /// <returns>
        /// The <see cref="JsonSerializerSettings"/> to use.
        /// </returns>
        private static JsonSerializerSettings ConfigureJsonFormatter(MvcJsonOptions options)
            => ConfigureJsonFormatter(options.SerializerSettings);

        /// <summary>
        /// Configures the JSON serializer.
        /// </summary>
        /// <param name="settings">The <see cref="JsonSerializerSettings"/> to configure.</param>
        /// <returns>
        /// The <see cref="JsonSerializerSettings"/> to use.
        /// </returns>
        private static JsonSerializerSettings ConfigureJsonFormatter(JsonSerializerSettings settings)
        {
            // Make JSON easier to read for debugging at the expense of larger payloads
            settings.Formatting = Formatting.Indented;

            // Explicitly define behavior when serializing DateTime values
            settings.DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ssK";   // Only return DateTimes to a 1 second precision

            settings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
            settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

            return settings;
        }

        /// <summary>
        /// Configures CORS.
        /// </summary>
        /// <param name="corsOptions">The <see cref="CorsOptions"/> to configure.</param>
        private void ConfigureCors(CorsOptions corsOptions)
        {
            var siteOptions = ServiceScope.ServiceProvider.GetService<SiteOptions>();

            corsOptions.AddPolicy(
                DefaultCorsPolicyName,
                (builder) =>
                {
                    builder
                        .WithExposedHeaders(siteOptions.Api.Cors.ExposedHeaders)
                        .WithHeaders(siteOptions.Api.Cors.Headers)
                        .WithMethods(siteOptions.Api.Cors.Methods);

                    if (HostingEnvironment.IsDevelopment())
                    {
                        builder.AllowAnyOrigin();
                    }
                    else
                    {
                        builder.WithOrigins(siteOptions.Api.Cors.Origins);
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
                DefaultContentType = "application/json",
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
                string extension = Path.GetExtension(context.File.PhysicalPath);

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

        /// <summary>
        /// Handles the application being stopped.
        /// </summary>
        private void OnApplicationStopped()
        {
            ServiceScope?.Dispose();
        }
    }
}
