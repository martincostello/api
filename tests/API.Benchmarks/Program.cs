// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api.Benchmarks
{
    using System;
    using System.Threading.Tasks;
    using BenchmarkDotNet.Running;

    /// <summary>
    /// A console application that runs performance benchmarks for the API. This class cannot be inherited.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// The main entry-point to the application.
        /// </summary>
        /// <param name="args">The arguments to the application.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous invocation of the application.
        /// </returns>
        internal static async Task Main(string[] args)
        {
            var switcher = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly);

            if (args?.Length == 0)
            {
                switcher.RunAll();
            }
            else if (args?.Length == 1 && string.Equals(args[0], "--test", StringComparison.OrdinalIgnoreCase))
            {
                using var benchmark = new ApiBenchmarks();
                await benchmark.StartServer();

                await benchmark.Hash();
                await benchmark.Time();

                await benchmark.StopServer();
            }
            else
            {
                switcher.Run(args);
            }
        }
    }
}
