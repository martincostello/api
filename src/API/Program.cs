// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="https://martincostello.com/">
//   Martin Costello (c) 2016
// </copyright>
// <summary>
//   Program.cs
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MartinCostello.Api
{
    using System;
    using System.IO;
    using System.Threading;
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
        public static void Main(string[] args)
        {
            // TODO Also use command-line arguments
            var builder = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>();

            using (CancellationTokenSource tokenSource = new CancellationTokenSource())
            {
                Console.CancelKeyPress += (_, e) =>
                {
                    tokenSource.Cancel();
                    e.Cancel = true;
                };

                using (var host = builder.Build())
                {
                    host.Run(tokenSource.Token);
                }
            }
        }
    }
}
