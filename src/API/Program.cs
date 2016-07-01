// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// A class representing the entry-point to the application. This class cannot be inherited.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Gets the current revision of the application.
        /// </summary>
        public static string Revision { get; } = FindCommitForAssembly();

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

                        host.Run(tokenSource.Token);
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

        /// <summary>
        /// Gets the Git commit SHA associated with this revision of the application.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> containing the Git SHA-1 for the revision of the application.
        /// </returns>
        private static string FindCommitForAssembly()
        {
            return typeof(Program).GetTypeInfo().Assembly
                .GetCustomAttributes<AssemblyMetadataAttribute>()
                .Where((p) => string.Equals(p.Key, "CommitSha", StringComparison.Ordinal))
                .Select((p) => p.Value)
                .DefaultIfEmpty("Local")
                .FirstOrDefault();
        }
    }
}
