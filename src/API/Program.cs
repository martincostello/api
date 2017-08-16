// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api
{
    using System;
    using System.Threading.Tasks;
    using Extensions;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;

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
        /// A <see cref="Task{TResult}"/> that returns the exit code from the application.
        /// </returns>
        public static async Task<int> Main(string[] args)
        {
            try
            {
                using (var host = BuildWebHost(args))
                {
                    await host.RunAsync();
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Unhandled exception: {ex}");
                return -1;
            }
        }

        /// <summary>
        /// Creates the web host to use for the application.
        /// </summary>
        /// <param name="args">The arguments to the application.</param>
        /// <returns>
        /// A <see cref="IWebHost"/> to use.
        /// </returns>
        private static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseKestrel((p) => p.AddServerHeader = false)
                .UseAzureAppServices()
                .UseStartup<Startup>()
                .CaptureStartupErrors(true)
                .Build();
        }
    }
}
