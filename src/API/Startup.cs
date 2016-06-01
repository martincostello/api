// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Startup.cs" company="https://martincostello.com/">
//   Martin Costello (c) 2016
// </copyright>
// <summary>
//   Startup.cs
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MartinCostello.Api
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Extensions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using NodaTime;

    /// <summary>
    /// A class representing the startup logic for the application.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="env">The <see cref="IHostingEnvironment"/> to use.</param>
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("hosting.json")
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets();
            }

            Configuration = builder.Build();
            HostingEnvironment = env;
        }

        /// <summary>
        /// Gets or sets the current configuration.
        /// </summary>
        public IConfigurationRoot Configuration { get; set; }

        /// <summary>
        /// Gets or sets the current hosting environment.
        /// </summary>
        public IHostingEnvironment HostingEnvironment { get; set; }

        /// <summary>
        /// Configures the application.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to use.</param>
        /// <param name="env">The <see cref="IHostingEnvironment"/> to use.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> to use.</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseCustomHttpHeaders(env.EnvironmentName, Configuration);

            if (env.IsDevelopment())
            {
                app.UseBrowserLink()
                   .UseDeveloperExceptionPage()
                   .UseRuntimeInfoPage();
            }
            else
            {
                app.UseExceptionHandler("/error")
                   .UseStatusCodePages(HandleError); // Use UseStatusCodePagesWithReExecute when query format supported
            }

            app.UseStaticFiles();

            app.UseForwardedHeaders(
                new ForwardedHeadersOptions()
                {
                    // Workaround for https://github.com/aspnet/IISIntegration/issues/140 in RC2
                    ForwardedHeaders = /*ForwardedHeaders.XForwardedFor |*/ ForwardedHeaders.XForwardedProto
                });

            app.UseHttpMethodOverride();

            app.UseMvc(
                (routes) =>
                {
                    routes.MapRoute(
                        name: "default",
                        template: "{controller=Home}/{action=Index}/{id?}");
                });
        }

        /// <summary>
        /// Configures the services for the application.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to use.</param>
        /// <returns>
        /// The <see cref="IServiceProvider"/> to use.
        /// </returns>
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(ConfigureMvc);

            services.AddRouting(
                (p) =>
                {
                    p.AppendTrailingSlash = true;
                    p.LowercaseUrls = true;
                });

            services.AddSingleton<IConfiguration>((_) => Configuration);
            services.AddSingleton<IClock>((_) => SystemClock.Instance);

            var builder = new ContainerBuilder();

            builder.Populate(services);

            var container = builder.Build();
            return container.Resolve<IServiceProvider>();
        }

        /// <summary>
        /// Configures the JSON output formatter for MVC.
        /// </summary>
        /// <param name="formatter">The <see cref="JsonOutputFormatter"/> to configure.</param>
        private static void ConfigureJsonFormatter(JsonOutputFormatter formatter)
        {
            // Serialize and deserialize JSON as "myProperty" => "MyProperty" -> "myProperty"
            formatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            // Make JSON easier to read for debugging at the expense of larger payloads
            formatter.SerializerSettings.Formatting = Formatting.Indented;

            // Omit nulls to reduce payload size
            formatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;

            // Explicitly define behavior when serializing DateTime values
            formatter.SerializerSettings.DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ssK";   // Only return DateTimes to a 1 second precision
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

            JsonOutputFormatter formatter = new JsonOutputFormatter();

            ConfigureJsonFormatter(formatter);

            options.OutputFormatters.Clear();
            options.OutputFormatters.Add(formatter);
        }

        /// <summary>
        /// An asynchronous handler for status code errors (such as 404).
        /// </summary>
        /// <param name="context">The status code context.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the error handler.
        /// </returns>
        private async Task HandleError(StatusCodeContext context)
        {
            var errorPath = new PathString("/error");
            var errorQueryString = new QueryString(string.Format(CultureInfo.InvariantCulture, "?id={0}", context.HttpContext.Response.StatusCode));

            var originalPath = context.HttpContext.Request.Path;
            var originalQueryString = context.HttpContext.Request.QueryString;

            context.HttpContext.Features.Set<IStatusCodeReExecuteFeature>(
                 new StatusCodeReExecuteFeature()
                 {
                     OriginalPathBase = context.HttpContext.Request.PathBase.Value,
                     OriginalPath = originalPath.Value
                 });

            context.HttpContext.Request.Path = errorPath;
            context.HttpContext.Request.QueryString = errorQueryString;

            try
            {
                await context.Next(context.HttpContext);
            }
            finally
            {
                context.HttpContext.Request.QueryString = originalQueryString;
                context.HttpContext.Request.Path = originalPath;
                context.HttpContext.Features.Set<IStatusCodeReExecuteFeature>(null);
            }
        }
    }
}
