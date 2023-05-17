﻿// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using MartinCostello.Logging.XUnit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

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

    /// <inheritdoc />
    public ITestOutputHelper? OutputHelper { get; set; }

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging((loggingBuilder) => loggingBuilder.ClearProviders().AddXUnit(this));
        builder.ConfigureServices((services) =>
        {
            var now = new DateTimeOffset(2016, 05, 24, 12, 34, 56, TimeSpan.Zero);

            var mock = new Mock<TimeProvider>();
            mock.Setup((p) => p.GetUtcNow()).Returns(now);

            services.AddSingleton<TimeProvider>(mock.Object);
        });
    }
}
