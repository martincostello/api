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
    using Microsoft.AspNet.Builder;
    using Microsoft.AspNet.Hosting;
    using Microsoft.AspNet.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

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
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        /// <summary>
        /// Gets or sets the current configuration.
        /// </summary>
        public IConfigurationRoot Configuration { get; set; }

        /// <summary>
        /// The main entry-point to the application.
        /// </summary>
        /// <param name="args">The arguments to the application.</param>
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);

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

            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseRuntimeInfoPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseIISPlatformHandler();

            app.UseStaticFiles();

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
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .AddJsonOptions(ConfigureJsonOptions);

            services.AddInstance<IConfiguration>(Configuration);
        }

        /// <summary>
        /// Configures the JSON options for MVC.
        /// </summary>
        /// <param name="options">The <see cref="MvcJsonOptions"/> to configure.</param>
        private static void ConfigureJsonOptions(MvcJsonOptions options)
        {
            // Serialize and deserialize JSON as "myProperty" => "MyProperty" -> "myProperty"
            options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            // Make JSON easier to read for debugging at the expense of larger payloads
            options.SerializerSettings.Formatting = Formatting.Indented;

            // Omit nulls to reduce payload size
            options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;

            // Explicitly define behavior when serializing DateTime values
            options.SerializerSettings.DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ssK";   // Only return DateTimes to a 1 second precision
        }
    }
}
