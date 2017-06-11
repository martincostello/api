﻿// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api
{
    using System;
    using System.IO;
    using System.Threading;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// A class representing the entry-point to the application. This class cannot be inherited.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry-point to the application.
        /// </summary>
        /// <param name="args">The arguments to the application.</param>
        /// <returns>
        /// The exit code from the application.
        /// </returns>
        public static int Main(string[] args) => Run(args);

        /// <summary>
        /// Runs ths application.
        /// </summary>
        /// <param name="args">The arguments to the application.</param>
        /// <param name="cancellationToken">The optional cancellation token to use.</param>
        /// <returns>
        /// The exit code from the application.
        /// </returns>
        public static int Run(string[] args, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var configuration = new ConfigurationBuilder()
                    .AddEnvironmentVariables()
                    .AddCommandLine(args)
                    .Build();

                var builder = new WebHostBuilder()
                    .UseKestrel((p) => p.AddServerHeader = false)
                    .UseConfiguration(configuration)
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseIISIntegration()
                    .UseStartup<Startup>()
                    .CaptureStartupErrors(true);

                using (var host = builder.Build())
                {
                    using (var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                    {
                        Console.CancelKeyPress += (_, e) =>
                        {
                            tokenSource.Cancel();
                            e.Cancel = true;
                        };

                        host.RunAsync(tokenSource.Token).GetAwaiter().GetResult();
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Unhandled exception: {ex}");
                return -1;
            }
        }
    }
}
