// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using JustEat.HttpClientInterception;
using MartinCostello.Logging.XUnit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;

namespace MartinCostello.Api.Integration;

/// <summary>
/// A class representing a factory for creating instances of the application.
/// </summary>
public class TestServerFixture : WebApplicationFactory<Models.TimeResponse>, ITestOutputHelperAccessor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestServerFixture"/> class.
    /// </summary>
    public TestServerFixture()
        : base()
    {
        ClientOptions.AllowAutoRedirect = false;
        ClientOptions.BaseAddress = new Uri("https://localhost");
    }

    /// <summary>
    /// Gets the <see cref="HttpClientInterceptorOptions"/> in use.
    /// </summary>
    public HttpClientInterceptorOptions Interceptor { get; } = new HttpClientInterceptorOptions().ThrowsOnMissingRegistration();

    /// <inheritdoc />
    public ITestOutputHelper? OutputHelper { get; set; }

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging((loggingBuilder) => loggingBuilder.ClearProviders().AddXUnit(this));
        builder.ConfigureServices((services) =>
        {
            var utcNow = new DateTimeOffset(2016, 05, 24, 12, 34, 56, TimeSpan.Zero);
            var timeProvider = new FakeTimeProvider(utcNow);

            services.AddSingleton<TimeProvider>(timeProvider);
            services.AddSingleton<IHttpMessageHandlerBuilderFilter>((_) => new HttpRequestInterceptionFilter(Interceptor));
        });
    }

    private sealed class HttpRequestInterceptionFilter(HttpClientInterceptorOptions options) : IHttpMessageHandlerBuilderFilter
    {
        public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
        {
            return (builder) =>
            {
                next(builder);
                builder.AdditionalHandlers.Add(options.CreateHttpMessageHandler());
            };
        }
    }
}
