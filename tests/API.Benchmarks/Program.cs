// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Running;
using MartinCostello.Api.Benchmarks;

#pragma warning disable SA1010
args ??= [];
#pragma warning restore SA1010

if (args.Length == 1 && string.Equals(args[0], "--test", StringComparison.OrdinalIgnoreCase))
{
    await using var benchmark = new ApiBenchmarks();
    await benchmark.StartServer();

    await benchmark.Hash();
    await benchmark.Time();

    await benchmark.StopServer();
}
else
{
    BenchmarkRunner.Run<ApiBenchmarks>(args: args);
}
